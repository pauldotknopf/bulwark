FROM microsoft/dotnet:2.1.1-runtime-bionic
RUN apt-get update
RUN apt -y install cmake libcurl4-openssl-dev libssl-dev pkg-config git
RUN apt-get clean
RUN git clone https://github.com/libgit2/libgit2.git /libgit2 && \
    cd /libgit2 && \
    git checkout b0d9952c318a3d1b8917e06ad46b9110c0c28831
RUN cd /libgit2 && \
    cmake -DCMAKE_BUILD_TYPE:STRING=Release \
      -DBUILD_CLAR:BOOL=OFF \
      -DUSE_SSH=OFF \
      -DENABLE_TRACE=ON \
      -DLIBGIT2_FILENAME=git2-b0d9952 \
      -DCMAKE_OSX_ARCHITECTURES="i386;x86_64" \
      . && \
      cmake --build .

FROM microsoft/dotnet:2.1.300-sdk-bionic
ADD webhook/linux-x64 /bulwark
# The libgit2sharp bundled libgit2 doesn't work, we use our own (built in another stage).
COPY --from=0 /libgit2/libgit2-b0d9952.so /bulwark
RUN mkdir /work
ENTRYPOINT ["/bulwark/Bulwark.Integration.WebHook"]
WORKDIR /work