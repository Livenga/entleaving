create extension if not exists "uuid-ossp";


create table if not exists readers(
  id            uuid                                               not null,
  hostname      character varying(256)                             not null,
  location_name character varying(128),
  remarks       character varying(128),
  created_at    timestamp with time zone default CURRENT_TIMESTAMP not null,
  primary key(id)
);


create table if not exists employees(
  id         integer                                            not null,
  tag_id     character varying(64),
  created_at timestamp with time zone default CURRENT_TIMESTAMP not null,
  primary key(id)
);


create table if not exists histories(
  id          bigserial                                          not null,
  reader_id   uuid                                               not null references readers(id),
  employee_id integer                                            not null references employees(id),
  status      smallint                                           not null,
  created_at  timestamp with time zone default CURRENT_TIMESTAMP not null,
  primary key(id)
);
