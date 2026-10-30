# snippet:image
FROM mcr.microsoft.com/dotnet/runtime:11.0.0-rc.2-azurelinux4.0@sha256:3b52667dbaff185a1ddd4cefb16d9517e6945f6e61257faa2e57e6040877d95b
WORKDIR /app
COPY publish/ ./
ENV QUAY_OS_VERSION=4.0
USER $APP_UID
ENTRYPOINT ["dotnet", "QuayWorker.dll"]
# endsnippet:image
