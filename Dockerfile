FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN apt-get update && apt-get install -y wget ca-certificates gnupg && \
    echo "deb [signed-by=/usr/share/keyrings/newrelic.gpg] https://apt.newrelic.com/debian/ newrelic non-free" > /etc/apt/sources.list.d/newrelic.list && \
    wget -O- https://download.newrelic.com/548C16BF.gpg | gpg --dearmor -o /usr/share/keyrings/newrelic.gpg && \
    apt-get update && apt-get install -y newrelic-dotnet-agent && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY ./publish/ ./

ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER="{36032161-FFC0-4B61-B559-F6C5D41BAE5A}"
ENV CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent
ENV CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so


ENTRYPOINT ["dotnet", "Cinema-Reservation.dll"]