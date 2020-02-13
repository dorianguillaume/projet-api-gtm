/*RESTORE BACKUP*/
/* restore database GrandHotel FROM DISK = N'C:\Users\Shadow\workspace\projetAPI-GTM\bdd\GrandHotel.bak'
with
move 'GrandHotel' to  N'C:\Users\Shadow\workspace\projetAPI-GTM\bdd\GrandHotel.mdf',
move 'GrandHotel_log' to  N'C:\Users\Shadow\workspace\projetAPI-GTM\bdd\GrandHotel.ldf' */

-- 1.	Les clients pour lesquels on n’a pas de numéro de portable (id, nom) 
-- Je pense OK
select C.Id, C.Nom, C.Prenom from Client C
inner join Telephone T on(C.Id = T.IdClient)
where CodeType = 'M'
Order by C.Id

-- 2.	Le taux moyen de réservation de l’hôtel par mois-année (2015-01, 2015-02…), c'est à dire la moyenne sur les chambres du ratio (nombre de jours de réservation dans le mois / nombre de jours du mois)
SELECT DAY(EOMONTH(GETDATE())) from Calendrier

-- Nbre de jour réservés pour une chambre par mois de 1 à 12, il faut trouver comment faire la retio du nombre de jour réserver avec le nbre de jour d'un mois de l'année
-- Il faut faire l'avg sur le nbre de jours dans un chaque mois
select CH.Numero, avg(month(CL.Jour)) as NbJours from Calendrier CL 
inner join Reservation R on(CL.Jour = R.Jour)
inner join Chambre CH on(R.NumChambre = CH.Numero)
group by month(CL.Jour), CH.Numero
order by CH.Numero

-- Ou... je n sais pas trop en fait cette requête donne un résultat bizarre, 6 partout en nombre de jours
select CH.Numero, avg(month(CL.Jour)) as NbJours from Calendrier CL 
inner join Reservation R on(CL.Jour = R.Jour)
inner join Chambre CH on(R.NumChambre = CH.Numero)
group by CH.Numero
order by CH.Numero

-- 3.	Le nombre total de jours réservés par les clients ayant une carte de fidélité au cours de la dernière année du calendrier (obtenue dynamiquement)
-- Je pense OK
select C.Id as ClientId, count(CL.Jour) as NbJours from Calendrier CL
inner join Reservation R on(CL.Jour = R.Jour)
inner join Client C on(R.IdClient = C.Id)
where C.CarteFidelite = 1 and year(CL.Jour) = (select top(1) year(CL.Jour) from Calendrier CL order by year(CL.Jour) desc) 
group by C.Id

-- 4.	Le chiffre d’affaire de l’hôtel par trimestre de chaque année

-- Chiffre d'affaire TTC: On tient compte de la réduction, celle-ci s'applique sur le prix HT et après on ajoute la TVA

-- Essais de sélection des trimestres

select F.DateFacture from Facture
group by (select month(F.DateFacture) from Facture)

select Dates Count(IIf([Dates] Between "01/01/2014" And "31/03/2014",[Dates])) AS [Matin]

select DateFacture from Facture
where DateFacture = (1-(Month([F.DateFacture]>5))

SELECT Month(DateFacture) Format("01/01/", "dd/mm/" & "31/03/", "dd/mm/")(Month([DateFacture])+2)\3 as Trimestre
FROM Facture

SELECT month(DateFacture) FROM Facture 
group by DateFacture between (format("01/01/", "dd/mm/") & format("31/03/", "dd/mm/"))

select DateFacture from Facture
where DateFacture between TO_DATE('01/01/','DD/MM/') & TO_DATE('31/03/','DD/MM/')

select DateFacture from Facture
group by quarter('dd/MM/yyyy')



-- Requête chiffre d'affaire
select (LF.MontantHT*(1 - LF.TauxReduction))*LF.TauxTVA as ChiffreDaffaire from Facture F
inner join LigneFacture LF on(F.Id = LF.IdFacture)
order by F.DateFacture, F.Id



-- 5.	Le nombre de clients dans chaque tranche de 1000 € de chiffre d’affaire total généré. La première tranche est < 5000 €, et la dernière >= 8000 €

select count(C.Id) from Client C
inner join Facture F on(C.Id = F.IdClient)
inner join LigneFacture LF on(F.Id = LF.DateFacture)
where (LF.MontantHT*(1 - LF.TauxReduction))*LF.TauxTVA as ChiffreDaffaire



-- 6.	Code T-SQL pour augmenter à partir du 01/01/2019 les tarifs des chambres de type 1 de 5%, et ceux des chambres de type 2 de 4% par rapport à l'année précédente


-- 7.	Clients qui ont passé au total au moins 7 jours à l’hôtel au cours d’un même mois (Id, Nom, mois où ils ont passé au moins 7 jours)


-- 8.	Clients qui sont restés à l’hôtel au moins deux jours de suite au cours de l’année 2017
