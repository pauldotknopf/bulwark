#!/usr/bin/env bash

mkdir -p $(pwd)/config
mkdir -p $(pwd)/logs
mkdir -p $(pwd)/data

docker run -it --rm \
	--hostname gitlab.example.com \
	--publish 443:443 --publish 80:80 \
	--name gitlab \
	--volume $(pwd)/config:/etc/gitlab \
	--volume $(pwd)/logs:/var/log/gitlab \
	--volume $(pwd)/data:/var/opt/gitlab \
	gitlab/gitlab-ee:11.9.0-rc3.ee.0
