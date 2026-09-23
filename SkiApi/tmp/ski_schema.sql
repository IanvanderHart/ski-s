
DROP TABLE IF EXISTS "SkiGrindHistory";
DROP TABLE IF EXISTS "SkiModels";
DROP TABLE IF EXISTS "StoneGrinds";


-- Справочник штайншлифтов
CREATE TABLE "StoneGrinds" (
    "Id" SERIAL PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "TempMin" DECIMAL,
    "TempMax" DECIMAL,
    "SnowTypes" TEXT,
    "TrackType" TEXT,
    "Notes" TEXT
);

-- Лыжи
CREATE TABLE "SkiModels" (
    "Id" SERIAL PRIMARY KEY,
    "Brand" TEXT NOT NULL,
    "Model" TEXT NOT NULL,
    "Year" INT,
    "Style" TEXT NOT NULL,              -- Free / Classic
    "Length" INT,
    "Profile" TEXT,                     -- s2, cold, blue, 61k
    "ProfileTempMin" DECIMAL,
    "ProfileTempMax" DECIMAL,
    "StiffnessValue" DECIMAL,            -- 100
    "StiffnessLabel" TEXT,               -- FA / MF / flex
    "CamberHeightMm" DECIMAL,
    "HasSkin" BOOLEAN NOT NULL DEFAULT FALSE,
    "StoneGrindId" INT REFERENCES "StoneGrinds"("Id"),
    "Notes" TEXT,
    "PersonalNotes" TEXT
);

-- История смены штайншлифтов (для аналитики)
CREATE TABLE "SkiGrindHistory" (
    "Id" SERIAL PRIMARY KEY,
    "SkiId" INT NOT NULL REFERENCES "SkiModels"("Id") ON DELETE CASCADE,
    "StoneGrindId" INT NOT NULL REFERENCES "StoneGrinds"("Id"),
    "FromDate" DATE NOT NULL,
    "ToDate" DATE,
    "Comment" TEXT
);

INSERT INTO "StoneGrinds" ("Name", "TempMin", "TempMax", "SnowTypes", "TrackType", "Notes")
VALUES
('P5-1',        -10,  5,  'All',                                    'Unprepared', 'Универсальный, для мягких трасс'),
('SL1',         -25, -10, 'FreshDry,FreshWet,OldDry',               'Prepared',   'Сухой снег, низкая влажность'),
('PF Diamant 6 pts', -5, -3, 'FreshDry,FreshWet,OldWet,Transformed','Prepared',   'Высокая влажность, перерождённый снег'),
('X3LS',         -2, 10,  'OldWet,Transformed',                     'Prepared',   'Тёплый, грязный, мокрый снег (вода)'),
('SO5',         -25, -3,  'All',                                    'All',        'Оптимально по свежему снегу'),
('9-6T',         -8,  1,  'FreshDry,FreshWet,OldDry',               'Prepared',   'Гололёд, свежий/старый снег');

INSERT INTO "SkiModels" ("Brand", "Model", "Year", "Style", "Length", "Profile", 
    "ProfileTempMin", "ProfileTempMax", "StiffnessValue", "StiffnessLabel", 
    "CamberHeightMm", "HasSkin", "StoneGrindId", "Notes", "PersonalNotes")
VALUES
('Fischer', 'Speedmax 3D plus skate 61k stiff', 2022, 'Free', 192, '61k', 
    NULL, NULL, 112, 'FA', 2.6, FALSE, 
    (SELECT "Id" FROM "StoneGrinds" WHERE "Name"='P5-1'),
    'Эпюра для мягких трасс. Штайншлифт P5-1',
    'Под спортсмена 92,76 кг в экипировке'),

('Salomon', 'S/Lab Carbon Skate Blue', 2023, 'Free', 192, 'blue', 
    NULL, NULL, 94, 'MF', 2.38, FALSE, 
    (SELECT "Id" FROM "StoneGrinds" WHERE "Name"='SL1'),
    'Эпюра для мягких трасс, штайншлифт SL1 (-10/-25)',
    'Под спортсмена 90,24 кг, сухой снег'),

('Rossignol', 'X-ium skating premium s2', 2022, 'Free', 192, 's2', 
    0, -10, 106, 'MF', 2.6, FALSE, 
    (SELECT "Id" FROM "StoneGrinds" WHERE "Name"='PF Diamant 6 pts'),
    'Универсальная эпюра, штайншлифт PF Diamant 6 pts',
    'Под спортсмена 101,76 кг, высокая влажность'),

('Rossignol', 'X-ium skating premium s3', 2020, 'Free', 188, 's3', 
    5, -2, 96, 'MF', 2.86, FALSE, 
    (SELECT "Id" FROM "StoneGrinds" WHERE "Name"='X3LS'),
    'Эпюра на тёплый/мокрый/грязный снег, штайншлифт X3LS',
    'Под спортсмена 92,16 кг, буквально вода'),

('Madshus', 'Redline 2.0 classic cold', 2018, 'Classic', 202, 'cold', 
    -5, -15, 52, 'flex', NULL, FALSE, 
    (SELECT "Id" FROM "StoneGrinds" WHERE "Name"='SO5'),
    'Универсальная эпюра для среднего холода и мороза, штайншлифт SO5',
    'Под спортсмена 93,6 кг, оптимально по свежему снегу. hbw неизвестен'),

('Madshus', 'Redline 2.0 classic intelligrip', 2018, 'Classic', 196, 'warm', 
    5, 0, 50, 'flex', NULL, TRUE, 
    (SELECT "Id" FROM "StoneGrinds" WHERE "Name"='9-6T'),
    'Эпюра для тёплых условий. Камус (intelligrip) — мази держания не нужны. Штайншлифт 9-6T',
    'Под спортсмена 93 кг, высокая влажность, подготовленная трасса. hbw неизвестен');

