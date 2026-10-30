# snippet:image
FROM mcr.microsoft.com/dotnet/runtime:10.0.13-azurelinux3.0@sha256:e454ed02b8c903dff0d7fd40a63451e55f16c186463d168a78e584738244f939
WORKDIR /app
COPY publish/ ./
ENV QUAY_OS_VERSION=3.0
USER $APP_UID
ENTRYPOINT ["dotnet", "QuayWorker.dll"]
# endsnippet:image
