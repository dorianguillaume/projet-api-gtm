/*RESTORE BACKUP*/
/* restore database GrandHotel FROM DISK = N'C:\Users\Shadow\workspace\projetAPI-GTM\bdd\GrandHotel.bak'
with
move 'GrandHotel' to  N'C:\Users\Shadow\workspace\projetAPI-GTM\bdd\GrandHotel.mdf',
move 'GrandHotel_log' to  N'C:\Users\Shadow\workspace\projetAPI-GTM\bdd\GrandHotel.ldf' */

-- 1.	Les clients pour lesquels on n’a pas de numéro de portable (id, nom) 

select * from Client C
inner join Telephone T on(C.Id = T.IdClient)

-- 2.	Le taux moyen de réservation de l’hôtel par mois-année (2015-01, 2015-02…), c'est à dire la moyenne sur les chambres du ratio (nombre de jours de réservation dans le mois / nombre de jours du mois)


-- 3.	Le nombre total de jours réservés par les clients ayant une carte de fidélité au cours de la dernière année du calendrier (obtenue dynamiquement)


-- 4.	Le chiffre d’affaire de l’hôtel par trimestre de chaque année


-- 5.	Le nombre de clients dans chaque tranche de 1000 € de chiffre d’affaire total généré. La première tranche est < 5000 €, et la dernière >= 8000 €


-- 6.	Code T-SQL pour augmenter à partir du 01/01/2019 les tarifs des chambres de type 1 de 5%, et ceux des chambres de type 2 de 4% par rapport à l'année précédente


-- 7.	Clients qui ont passé au total au moins 7 jours à l’hôtel au cours d’un même mois (Id, Nom, mois où ils ont passé au moins 7 jours)


-- 8.	Clients qui sont restés à l’hôtel au moins deux jours de suite au cours de l’année 2017
