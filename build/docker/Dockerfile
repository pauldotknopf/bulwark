FROM mcr.microsoft.com/dotnet/core/runtime:2.2.3-bionic
ADD webhook/linux-x64 /bulwark
RUN mkdir /work
EXPOSE 5000
ENTRYPOINT ["/bulwark/Bulwark.Integration.WebHook"]
WORKDIR /work