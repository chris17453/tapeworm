FROM centos

ADD app /app/
ADD rpm /rpm/
RUN chmod +x /app/tapeworm_webAPI

# RUN yum install -y libunwind libicu
RUN rpm -ihv /rpm/libunwind-1.2-2.el7.x86_64.rpm
RUN rpm -ihv /rpm/libicu-50.1.2-15.el7.x86_64.rpm

VOLUME /app/config
VOLUME /app/data
WORKDIR /app

EXPOSE 5001
#ENTRYPOINT ["/bin/bash"]
ENTRYPOINT ["/app/tapeworm_webAPI"]
