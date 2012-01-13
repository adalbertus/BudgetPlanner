UPDATE [Configuration] SET [Value] = 'http://budzet-domowy.pietkiewicz.pl/pomoc' WHERE [Key] = 'HelpPage';
UPDATE [Configuration] SET [Value] = 'http://budzet-domowy.pietkiewicz.pl/online-update' WHERE [Key] = 'UpdatePage';
UPDATE [Configuration] SET [Value] = '2' WHERE [Key] = 'DatabaseVersion';

ALTER TABLE [Saving] ADD COLUMN [StartingBalance] NUMERIC NOT NULL DEFAULT 0;