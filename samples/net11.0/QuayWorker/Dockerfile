# snippet:image
FROM mcr.microsoft.com/dotnet/runtime:11.0.0-rc.3-azurelinux4.0@sha256:811515cabb3edf04bbbef8da41adbd51beb7f973716046d80de77822d2c1dece
WORKDIR /app
COPY publish/ ./
ENV QUAY_OS_VERSION=4.0
USER $APP_UID
ENTRYPOINT ["dotnet", "QuayWorker.dll"]
# endsnippet:image
