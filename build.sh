docker build -t my_zhi_hu_api -f ./MyZhiHuAPI/Dockerfile .
docker create --name my_zhi_hu_api -p 8888:8888 -e ASPNETCORE_URLS="https://+" -e ASPNETCORE_HTTPS_PORTS=8888 -e ASPNETCORE_Kestrel__Certificates__Default__Password="abc123" -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx -v MyZhiHuAPIConfig:/https my_zhi_hu_api
docker start --restart=always my_zhi_hu_api
