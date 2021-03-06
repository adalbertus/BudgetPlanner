﻿INSERT INTO Configuration VALUES('DatabaseVersion','1','3','Wersja bazy danych');
INSERT INTO Configuration VALUES('IsFirstRun','1','True','Czy jest to pierwsze uruchomienie aplikacji');
INSERT INTO Configuration VALUES('AuthorEmail','1','budzet-domowy@@pietkiewicz.pl','Adres autora aplikacji');
INSERT INTO Configuration VALUES('HomePage','1','http://budzet-domowy.pietkiewicz.pl/','Adres strony WWW aplikacji');
INSERT INTO Configuration VALUES('HelpPage','1','http://budzet-domowy.pietkiewicz.pl/pomoc','Adres strony WWW z pomocą');
INSERT INTO Configuration VALUES('UpdatePage','1','http://budzet-domowy.pietkiewicz.pl/online-update','Adres strony z informacjami o aktualizacjach');
INSERT INTO Configuration VALUES('UpdateMinutesInterval','1','15','Co ile minut aplikacja ma sprawdzać aktualizacje');

INSERT INTO CashFlowGroup VALUES(1,'Dom','Wszystkie wydatki związane z domem','0',2);
INSERT INTO CashFlowGroup VALUES(2,'Oszczędności',NULL,'1',11);
INSERT INTO CashFlowGroup VALUES(3,'Osobiste','Nasze ubrania, usługi, kosmetyki, kieszonkowe','0',3);
INSERT INTO CashFlowGroup VALUES(4,'Dzieci','Ubranka, pielęgnacja, leki, wyjazdy','0',4);
INSERT INTO CashFlowGroup VALUES(5,'Transport','Bilety, paliwo','0',5);
INSERT INTO CashFlowGroup VALUES(6,'Zdrowie',NULL,'0',6);
INSERT INTO CashFlowGroup VALUES(7,'Rozrywka i rekreacja','Sport, kino','0',7);
INSERT INTO CashFlowGroup VALUES(8,'Żywność',NULL,'0',1);
INSERT INTO CashFlowGroup VALUES(9,'Dług',NULL,'0',8);
INSERT INTO CashFlowGroup VALUES(10,'Koty',NULL,'0',9);
INSERT INTO CashFlowGroup VALUES(11,'Dary',NULL,'0',10);

INSERT INTO CashFlow VALUES(1,1,'Chemia',NULL);
INSERT INTO CashFlow VALUES(2,1,'Wystrój',NULL);
INSERT INTO CashFlow VALUES(3,1,'Opłaty za mieszkanie',NULL);
INSERT INTO CashFlow VALUES(4,11,'Pomoc bliźnim',NULL);
INSERT INTO CashFlow VALUES(5,11,'Prezenty','Taca, pomoc Ligocie, itp');
INSERT INTO CashFlow VALUES(6,11,'Na kościół',NULL);
INSERT INTO CashFlow VALUES(7,5,'Paliwo',NULL);
INSERT INTO CashFlow VALUES(8,8,'Artykuły spożywcze',NULL);
INSERT INTO CashFlow VALUES(9,5,'Bilety MPK',NULL);
INSERT INTO CashFlow VALUES(10,5,'Inne','Myjnia, opłaty parkingowe, inny transport');
INSERT INTO CashFlow VALUES(11,9,'Kredyt hipoteczny',NULL);
INSERT INTO CashFlow VALUES(12,10,'Jedzenie',NULL);
INSERT INTO CashFlow VALUES(13,10,'Żwirek',NULL);
INSERT INTO CashFlow VALUES(14,6,'Dentysta',NULL);
INSERT INTO CashFlow VALUES(15,6,'Lekarstwa',NULL);
INSERT INTO CashFlow VALUES(16,7,'Randki',NULL);
INSERT INTO CashFlow VALUES(17,7,'Wyjazdy',NULL);
INSERT INTO CashFlow VALUES(18,7,'Sporty regularne',NULL);
INSERT INTO CashFlow VALUES(19,7,'Wspólne wyjścia rodzinne',NULL);
INSERT INTO CashFlow VALUES(20,4,'Przedszkole ',NULL);
INSERT INTO CashFlow VALUES(21,4,'Zajęcia dzieci/opieka',NULL);
INSERT INTO CashFlow VALUES(22,4,'Ubrania dzieci',NULL);
INSERT INTO CashFlow VALUES(23,4,'Kosmetyki, pieluchy ',NULL);
INSERT INTO CashFlow VALUES(24,4,'Sprzęt i zabawki dzieci',NULL);
INSERT INTO CashFlow VALUES(25,3,'Higiena osobista',NULL);
INSERT INTO CashFlow VALUES(26,3,'Kieszonkowe',NULL);