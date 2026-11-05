# snippet:image
FROM mcr.microsoft.com/dotnet/runtime:10.0.14-azurelinux3.0@sha256:6b0cde57a2d55aa6ef5b9c9eafbc156e3c0e0b7c114134111103bc0a3e474517
WORKDIR /app
COPY publish/ ./
ENV QUAY_OS_VERSION=3.0
USER $APP_UID
ENTRYPOINT ["dotnet", "QuayWorker.dll"]
# endsnippet:image
