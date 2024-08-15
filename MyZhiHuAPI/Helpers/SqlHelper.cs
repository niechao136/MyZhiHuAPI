namespace MyZhiHuAPI.Helpers;

public static class SqlHelper
{
    #region User
    public const string UserReturn =
        """
        users.id, users.username, users.role, users.nickname, users.email, users.phone, users.questions, users.answers,
        users.commits, users.remarks, users.watching_people, users.watching_question, users.update_at, users.create_at
        """;
    #endregion
    #region Question

    private const string QuestionReturn =
        """
        questions.id, questions.title, questions.content, questions.owner_id, questions.answers,
        questions.watching, questions.update_at, questions.create_at
        """;

    public static string QuestionList(int? ownerId, out string count)
    {
        const string isDelete = "is_delete = FALSE";
        var filter = ownerId == null ? "" : $" AND owner_id = {ownerId}";
        count = $"SELECT COUNT(DISTINCT id) FROM questions WHERE {isDelete + filter}";
        return $"SELECT {QuestionReturn} FROM questions WHERE {isDelete + filter} ORDER BY update_at LIMIT @limit OFFSET @offset";
    }
    public const string QuestionInsert =
        $"""
        INSERT INTO questions (title, content, owner_id) 
        VALUES (@title, @content, @ownerId) RETURNING {QuestionReturn}
        """;
    public const string QuestionUpdate =
        $"""
        UPDATE questions SET title = @title, content = @content, update_at = NOW()
        WHERE id = @id RETURNING {QuestionReturn}
        """;
    public const string QuestionDelete = "UPDATE questions SET is_delete = TRUE, update_at = NOW() WHERE id = @id";
    public static string QuestionWatch(bool cancel, string param)
    {
        var action = cancel ? "array_remove" : "array_append";
        return $"UPDATE questions SET {param} = {action}({param}, @ownerId::INTEGER) WHERE id = @id RETURNING {QuestionReturn}";
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
    public const string AnswerDelete = "UPDATE answers SET is_delete = TRUE, update_at = NOW() WHERE id = @id";
    // 点赞/取消点赞回答 与 收藏/取消收藏回答
    public static string AnswerAgree(bool cancel, string param)
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
        commits.parent_id, commits.update_at, commits.create_at
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
