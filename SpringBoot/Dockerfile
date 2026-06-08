# Build assumes a standard Maven layout under this directory: pom.xml, src/, etc.
# Runtime listens on 5000 (same as the Python backend) for nginx / compose parity.

FROM eclipse-temurin:23-jdk-noble AS build
RUN apt-get update \
    && apt-get install -y --no-install-recommends maven \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY pom.xml .
COPY src ./src
RUN mvn -B -DskipTests package

FROM eclipse-temurin:23-jre-noble
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/target/*.jar app.jar

ENV SERVER_PORT=5000
ENV JAVA_TOOL_OPTIONS="-Xms128m -Xmx384m -XX:+UseSerialGC"
EXPOSE 5000

# curl only (no Python); try actuator / app / then root (shell form so || works)
HEALTHCHECK --interval=15s --timeout=10s --start-period=120s --retries=5 \
  CMD curl -fsS http://127.0.0.1:5000/health >/dev/null 2>&1 || curl -fsS http://127.0.0.1:5000/actuator/health >/dev/null 2>&1 || curl -fsS -o /dev/null http://127.0.0.1:5000/ || exit 1

ENTRYPOINT ["java", "-jar", "app.jar"]
