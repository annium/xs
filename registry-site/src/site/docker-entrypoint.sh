#!/usr/bin/env sh
set -e

echo -e "`/sbin/ip route|awk '/default/ { print $3 }'`\thost.docker.internal" >> /etc/hosts

nginx -g 'daemon off;'