FROM centos

ADD app /app/
RUN chmod +x /app/tapeworm_webAPI
RUN yum install -y libunwind libicu

VOLUME /app/config
VOLUME /app/data
WORKDIR /app

EXPOSE 5001
#ENTRYPOINT ["/bin/bash"]
ENTRYPOINT ["/app/tapeworm_webAPI"]
