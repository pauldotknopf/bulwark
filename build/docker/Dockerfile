FROM microsoft/dotnet:2.1.300-rc1-sdk-bionic
ADD webhook/linux-x64 /bulwark
RUN mkdir /work
EXPOSE 5000
ENTRYPOINT ["/bulwark/Bulwark.Integration.WebHook"]
WORKDIR /work
