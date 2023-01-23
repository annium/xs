-- admins
insert into public.users
    (id, email, login, password_hash, referral_id)
values
    ('baa0ad0f-91c5-4c19-963c-ea369048e67a', 'a.kreskiyan@gmail.com', 'alex', 'ohraPG8QMZiOnXX+MWh/45aZDwjtv/7FQMFzXxSRxQjLdSMBHpELKDSznF6cSUalufovlgCfFkn4mtR7eXB+8w==', null);

-- id app
insert into public.apps
    (id, api_token, name, owner_id)
values
    ('278e20ae-00c7-4ba5-8db3-55df7af12d44', 'b62acd2a-2f1b-4da1-9273-abab4b9da7f7', 'Annium ID', 'baa0ad0f-91c5-4c19-963c-ea369048e67a')
