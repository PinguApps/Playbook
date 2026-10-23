# Canonical collector configuration

<!-- snippet:collector -->
receivers:
  otlp:
    protocols:
      http:
        endpoint: 0.0.0.0:4318
        cors:
          allowed_origins: [http://localhost:5931, http://localhost:5932]
          allowed_headers: [Content-Type]
exporters:
  debug:
    verbosity: detailed
  file:
    path: /receipts/otlp.json
service:
  pipelines:
    traces:
      receivers: [otlp]
      exporters: [debug, file]
    metrics:
      receivers: [otlp]
      exporters: [debug, file]
<!-- endsnippet:collector -->
