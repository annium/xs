FROM node:alpine as builder
COPY . /src/
WORKDIR /src/site.lib/src
RUN yarn install --force
RUN yarn build
WORKDIR /src/site/src
RUN yarn install --force
RUN yarn build


FROM nginx:alpine
COPY --from=builder /src/site/build /src/site/docker-entrypoint.sh /site/
RUN rm /etc/nginx/conf.d/default.conf
VOLUME [ "/etc/nginx/conf.d/default.conf" ]
EXPOSE 80
WORKDIR /site
ENTRYPOINT [ "./docker-entrypoint.sh" ]