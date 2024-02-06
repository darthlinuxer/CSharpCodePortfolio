# Dockerfile
FROM ravendb/ravendb:4.2.123-ubuntu.18.04-x64

# Install Java
RUN apt-get update && \
    apt-get install -y openjdk-11-jdk && \
    apt-get clean;

# Download BouncyCastle
ADD https://www.bouncycastle.org/download/bcprov-jdk15on-169.jar /usr/lib/jvm/java-11-openjdk-amd64/lib/ext/

# Set BouncyCastle as a Java security provider
RUN echo 'security.provider.10=org.bouncycastle.jce.provider.BouncyCastleProvider' >> /etc/java-11-openjdk/security/java.security

# Set the classpath to include BouncyCastle
ENV CLASSPATH=/usr/lib/jvm/java-11-openjdk-amd64/lib/ext/bcprov-jdk15on-169.jar:$CLASSPATH
