# Author-maintained verification baseline. Update engine and FTS together and rerun Test.
FROM mcr.microsoft.com/mssql/server@sha256:245f6b1c8903edae52cb20fa31419cefcde1a49938bb8c3447099011fc871ca3
USER root
RUN echo 'deb [arch=amd64 signed-by=/usr/share/keyrings/microsoft-prod.gpg] https://packages.microsoft.com/ubuntu/24.04/mssql-server-2025 noble main' > /etc/apt/sources.list.d/mssql-server.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y --no-install-recommends mssql-server=17.0.5035.5-3 mssql-server-fts=17.0.5035.5-3 \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*
USER mssql
