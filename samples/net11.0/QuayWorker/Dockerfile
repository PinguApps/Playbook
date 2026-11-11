# snippet:image
FROM mcr.microsoft.com/dotnet/runtime:11.0.0-azurelinux4.0@sha256:dd668717849c0a887c646b2ea3d3282277631273e5748de42e12b3a5c5ffdf9d
WORKDIR /app
COPY publish/ ./
ENV QUAY_OS_VERSION=4.0
USER $APP_UID
ENTRYPOINT ["dotnet", "QuayWorker.dll"]
# endsnippet:image
