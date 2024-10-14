using JiebaNet.Segmenter;
using JiebaNet.Segmenter.Common;

namespace MyZhiHuAPI.Helpers;

public static class SqlHelper
{
    #region User

    private const string UserReturn =
        """
        users.id, users.username, users.role, users.nickname, users.email, users.phone,
        users.questions, users.answers, users.commits, users.remarks, users.watching_people,
        users.watching_question, users.subscribers, users.update_at, users.create_at
        """;

    public const string UserInfo = $"SELECT {UserReturn} FROM users WHERE id = @id";
    public const string UserInsert =
        """
        INSERT INTO users (username, password, role, nickname, email, phone) 
        VALUES (@username, @password, @role, @nickname, @email, @phone)
        """;

    public const string UserAdd = $"{UserInsert} RETURNING {UserReturn}";
    public const string UserUpdate =
        $"""
        UPDATE users SET nickname = @nickname, email = @email, phone = @phone, update_at = NOW() WHERE id = @id
        RETURNING {UserReturn}
        """;
    public const string UserDelete =
        $"UPDATE users SET is_delete = TRUE, update_at = NOW() WHERE id = @id RETURNING {UserReturn}";
    public const string UserQuery = "SELECT id FROM users WHERE username = @username AND is_delete = FALSE";
    public const string UserLogin =
        "SELECT id, role FROM users WHERE username = @username AND password = @password AND is_delete = FALSE";

    public static string UserSet(bool cancel, string param, bool batch = false)
    {
        var action = cancel ? "array_remove" : "array_append";
        var value = batch ? "ANY(@ids)" : "@id";
        return $"UPDATE users SET {param} = {action}({param}, @target::INTEGER) WHERE id = {value} RETURNING {UserReturn}";
    }
    #endregion
    #region Question

    private const string QuestionReturn =
        """
        questions.id, questions.title, questions.content, questions.owner_id, questions.answers,
        questions.watching, questions.update_at, questions.create_at
        """;

    public static string QuestionList(int? ownerId, string? keyword, out string count)
    {
        const string isDelete = "is_delete = FALSE";
        var filter = ownerId == null ? "" : $" AND owner_id = {ownerId}";
        if (keyword == null)
        {
            count = $"SELECT COUNT(DISTINCT id) FROM questions WHERE {isDelete + filter}";
            return $"SELECT {QuestionReturn} FROM questions WHERE {isDelete + filter} ORDER BY update_at LIMIT @limit OFFSET @offset";
        }
        var segmenter = new JiebaSegmenter();
        var search = segmenter.CutForSearch(keyword.Trim()).Where(o => o.Trim() != "").Join("|");
        count = $"SELECT COUNT(DISTINCT id) FROM questions WHERE {isDelete + filter} AND questions.search @@ to_tsquery('{search}')";
        return
            $"""
            SELECT {QuestionReturn} FROM questions, to_tsquery('{search}') query WHERE {isDelete + filter} 
            AND questions.search @@ query ORDER BY ts_rank(questions.search, query) DESC LIMIT @limit OFFSET @offset
            """;
    }
    public const string QuestionInsert =
        $"""
        INSERT INTO questions (title, content, owner_id, search) 
        VALUES (@title, @content, @ownerId, @search::tsvector) RETURNING {QuestionReturn}
        """;
    public const string QuestionUpdate =
        $"""
        UPDATE questions SET title = @title, content = @content, update_at = NOW()
        WHERE id = @id RETURNING {QuestionReturn}
        """;
    public const string QuestionDelete =
        $"UPDATE questions SET is_delete = TRUE, update_at = NOW() WHERE id = @id RETURNING {QuestionReturn}";
    public static string QuestionSet(bool cancel, string param)
    {
        var action = cancel ? "array_remove" : "array_append";
        return $"UPDATE questions SET {param} = {action}({param}, @target::INTEGER) WHERE id = @id RETURNING {QuestionReturn}";
    }
        
    #endregion
    #region Answer

    private const string AnswerReturn =
        """
        answers.id, answers.content, answers.owner_id, answers.question_id, answers.commits,
        answers.remark, answers.update_at, answers.create_at
        """;
    // 获取回答列表
    public static string AnswerList(int? questionId, out string count)
    {
        const string isDelete = "answers.is_delete = FALSE";
        var filter = questionId == null ? "" : $" AND answers.question_id = {questionId}";
        count = $"SELECT COUNT(DISTINCT id) FROM answers WHERE {isDelete + filter}";
        return $"""
                SELECT {AnswerReturn}, questions.title
                FROM answers LEFT JOIN questions ON questions.id = answers.question_id
                WHERE {isDelete + filter} ORDER BY update_at LIMIT @limit OFFSET @offset
                """;
    }
    // 新增回答
    public const string AnswerInsert =
        $"""
        INSERT INTO answers (content, owner_id, question_id) 
        VALUES (@content, @ownerId, @questionId) RETURNING {AnswerReturn}
        """;
    // 修改回答
    public const string AnswerUpdate =
        $"UPDATE answers SET content = @content, update_at = NOW() WHERE id = @id RETURNING {AnswerReturn}";
    // 删除回答
    public static string AnswerDelete(string param = "id") {
        return $"UPDATE answers SET is_delete = TRUE, update_at = NOW() WHERE {param} = @id RETURNING {AnswerReturn}";
    }
    // 点赞/取消点赞回答 与 收藏/取消收藏回答
    public static string AnswerSet(bool cancel, string param)
    {
        var action = cancel ? "array_remove" : "array_append";
        return $"UPDATE answers SET {param} = {action}({param}, @ownerId::INTEGER) WHERE id = @id RETURNING {AnswerReturn}";
    }
    // 获取单笔答案信息
    public const string AnswerInfo = $"SELECT {AnswerReturn} FROM answers WHERE id = @id";
    #endregion
    #region Commit
    private const string CommitReturn =
        """
        commits.id, commits.content, commits.owner_id, commits.answer_id, commits.root_id,
        commits.parent_id, commits.children, commits.agree, commits.update_at, commits.create_at
        """;
    // 获取评论列表
    public const string CommitList =
        $"""
         SELECT {CommitReturn}, users.nickname, parent.nickname AS parent FROM commits
             LEFT JOIN users ON users.id = commits.owner_id
             LEFT JOIN users AS parent ON parent.id = commits.parent_id
         WHERE answer_id = @answerId AND root_id = @rootId
         ORDER BY update_at LIMIT @limit OFFSET @offset
         """;
    public const string CommitCount =
        "SELECT COUNT(DISTINCT id) FROM commits WHERE answer_id = @answerId AND root_id = @rootId AND is_delete = FALSE";
    public const string CommitInsert =
        $"""
        INSERT INTO commits (content, owner_id, answer_id, root_id, parent_id) 
        VALUES (@content, @ownerId, @answerId, @rootId, @parentId) RETURNING {CommitReturn}
        """;
    public const string CommitInfo = $"SELECT {CommitReturn} FROM commits WHERE id = @id";

    public static string CommitSet(bool cancel, string param)
    {
        var action = cancel ? "array_remove" : "array_append";
        return $"UPDATE commits SET {param} = {action}({param}, @ownerId::INTEGER) WHERE id = @id RETURNING {CommitReturn}";
    }

    public static string CommitDelete(string param = "id", bool batch = false)
    {
        var value = batch ? "ANY(@ids)" : "@id";
        return $"UPDATE commits SET is_delete = TRUE, update_at = NOW() WHERE {param} = {value}";
    }
    #endregion
    #region Notify
    // 新增通知
    public const string NotifyInsert =
        """
        INSERT INTO notifies (owner_id, operate_id, target_id, type)
        VALUES (@owner_id, @operate_id, @target_id, @type)
        RETURNING id, owner_id, operate_id, target_id, type, create_at, update_at
        """;
    // 获取通知列表
    public const string NotifyList =
        """
        SELECT id, owner_id, operate_id, target_id, type, create_at, update_at
        FROM notifies WHERE is_delete = FALSE AND owner_id = @id ORDER BY update_at LIMIT @limit OFFSET @offset
        """;

    public const string NotifyCount = "SELECT COUNT(DISTINCT id) FROM notifies WHERE is_delete = FALSE AND owner_id = @id";
    // 用于新增回答的通知
    public const string NotifyAnswer =
        """
        SELECT owner_id, users.nickname, title FROM questions LEFT JOIN users ON users.id = @owner_id
        WHERE questions.id = @id
        """;
    // 用于新增评论的通知
    public const string NotifyCommit =
        """
        SELECT owner_id, users.nickname FROM answers LEFT JOIN users ON users.id = @owner_id
        WHERE answers.id = @id
        """;
    // 用于赞同回答的通知
    public const string NotifyAgree = "SELECT nickname FROM users WHERE id = @id";
    #endregion
}
