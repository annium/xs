create table public.users (
	id uuid not null,
	login text not null,
	password_hash text not null,
	api_token uuid not null,
	constraint pk_users primary key (id)
);
create unique index ix_users_login on public.users using btree (login);