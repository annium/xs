create table public.user_sessions (
	id uuid not null,
	user_id uuid not null,
	token uuid not null,
	expires timestamptz not null,
	constraint pk_user_logins primary key (id),
	constraint fk_user_logins_users_user_id foreign key (user_id) references public.users(id) on delete restrict
);
create index ix_user_logins_user_id on public.user_sessions using btree (user_id);
create unique index ix_user_logins_token on public.user_sessions using btree (token);
