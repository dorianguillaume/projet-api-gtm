/*RESTORE BACKUP*/
/* restore database GrandHotel FROM DISK = N'C:\Users\Shadow\workspace\projetAPI-GTM\bdd\GrandHotel.bak'
with
move 'GrandHotel' to  N'C:\Users\Shadow\workspace\projetAPI-GTM\bdd\GrandHotel.mdf',
move 'GrandHotel_log' to  N'C:\Users\Shadow\workspace\projetAPI-GTM\bdd\GrandHotel.ldf' */

-- 1.	Les clients pour lesquels on n’a pas de numéro de portable (id, nom) 
select C.Id, C.Nom from Client C
join Telephone T on (C.Id = T.IdClient)
where C.Id not in (select IdClient from Telephone where CodeType = 'M');

-- 2.	Le taux moyen de réservation de l’hôtel par mois-année (2015-01, 2015-02…), c'est à dire la moyenne sur les chambres du ratio (nombre de jours de réservation dans le mois / nombre de jours du mois)
select Mois, Annee, avg(MoyenneMois) 
from (select month(CL.Jour) as Mois, year(CL.Jour) as Annee, cast(count(day(CL.Jour)) as decimal)/CAST(Day(EOMONTH(CL.Jour))as Int) as MoyenneMois from Calendrier CL 
inner join Reservation R on(CL.Jour = R.Jour)
inner join Chambre CH on(R.NumChambre = CH.Numero)
group by month(CL.Jour), year(CL.Jour), CH.Numero, (Day(EOMONTH(CL.Jour)))) as m
group by Mois, Annee
order by Annee, Mois


-- 3.	Le nombre total de jours réservés par les clients ayant une carte de fidélité au cours de la dernière année du calendrier (obtenue dynamiquement)
select count(*) as NombreTotalJours from Calendrier ca
join Reservation r on ca.Jour = r.Jour
join Client cl on r.IdClient = cl.Id
where cl.CarteFidelite = 1 and year(ca.Jour) = (select top(1) year(Jour) from Calendrier order by year(Jour) desc) 

-- 4.	Le chiffre d’affaire de l’hôtel par trimestre de chaque année
-- Chiffre d'affaire TTC: On tient compte de la réduction, celle-ci s'applique sur le prix HT et après on ajoute la TVA
-- Essais de sélection des trimestres

select year(f.DateFacture), datepart(qq, f.DateFacture), sum(MontantHT*(1-TauxReduction)*(1+TauxTVA)*Quantite) as Total 
from LigneFacture lf
join Facture f on lf.IdFacture = f.Id
group by year(f.DateFacture), datepart(qq, f.DateFacture)
order by year(f.DateFacture), datepart(qq, f.DateFacture)

-- 5.	Le nombre d'argent qu'a depensé chaque client / qu'on regroupe par tranche
-- Le nombre de clients dans chaque tranche de 1000 € de chiffre d’affaire total généré. La première tranche est < 5000 €, et la dernière >= 8000 €
-- PAS GOOD
select *

from (

	select '5000 et Moins' as Tranche, count(SommeClients) AS NbClient
	from (select SUM(f.IdClient) as SommeClients 
	from LigneFacture lf
	join Facture f on lf.IdFacture = f.Id
	group by f.IdClient
	HAVING sum(MontantHT*(1-TauxReduction)*(1+TauxTVA)*Quantite) < 5000) as Clients

	UNION

	select '5000-6000' as Tranche, count(Tranche1) AS NbClient
	from (select SUM(f.IdClient) as Tranche1 
	from LigneFacture lf
	join Facture f on lf.IdFacture = f.Id
	group by f.IdClient
	HAVING sum(MontantHT*(1-TauxReduction)*(1+TauxTVA)*Quantite) BETWEEN 5000 AND 6000) as Clients

	UNION 

	select '6000-7000' as Tranche, count(SommeClients) AS NbClient
	from (select SUM(f.IdClient) as SommeClients 
	from LigneFacture lf
	join Facture f on lf.IdFacture = f.Id
	group by f.IdClient
	HAVING sum(MontantHT*(1-TauxReduction)*(1+TauxTVA)*Quantite) BETWEEN 6000 AND 7000) as Clients

	UNION 

	select '7000-8000' as Tranche, count(SommeClients) AS NbClient
	from (select SUM(f.IdClient) as SommeClients 
	from LigneFacture lf
	join Facture f on lf.IdFacture = f.Id
	group by f.IdClient
	HAVING sum(MontantHT*(1-TauxReduction)*(1+TauxTVA)*Quantite) BETWEEN 7000 AND 8000) as Clients

	UNION 

	select '8000 et Plus' as Tranche, count(SommeClients) AS NbClient
	from (select SUM(f.IdClient) as SommeClients 
	from LigneFacture lf
	join Facture f on lf.IdFacture = f.Id
	group by f.IdClient
	HAVING sum(MontantHT*(1-TauxReduction)*(1+TauxTVA)*Quantite) > 8000) as Clients

) as Ratio

-- 6.	Code T-SQL pour augmenter à partir du 01/01/2019 les tarifs des chambres de type 1 de 5%, et ceux des chambres de type 2 de 4% par rapport à l'année précédente
-- Demander si il faut modifier TarifChambre ???
declare @PrixType1 int;
declare @PrixType2 int;

set @PrixType1 = (
    select sum((P) * (1.05))
    from(
        select Prix as P 
        from Tarif 
        where Code = 'CHB1-2018') 
        as prix
    );
set @PrixType2 = (
select sum((P) * (1.04))
from(
    select Prix as P 
    from Tarif 
    where Code = 'CHB2-2018') 
    as prix
);

insert into Tarif(Code, DateDebut, Prix) values('CHB1-2019', '2019-01-01', @PrixType1)
insert into Tarif(Code, DateDebut, Prix) values('CHB2-2019', '2019-01-01', @PrixType2)

--insert into TarifChambre select NumChambre from TarifChambre like 'CHB1%'

-- 7.	Clients qui ont passé au total au moins 7 jours à l’hôtel au cours d’un même mois (Id, Nom, mois où ils ont passé au moins 7 jours)
select cl.Id, cl.Nom, Month(ca.Jour) as Mois from Client cl
join Reservation r on cl.Id = r.IdClient
join Calendrier ca on r.Jour = ca.Jour
group by cl.Id, cl.Nom,  Month(ca.Jour), Year(ca.Jour)
Having count(Month(ca.Jour)) >= 7;

-- 8.	Clients qui sont restés à l’hôtel au moins deux jours de suite au cours de l’année 2017
SELECT distinct Id
from reservation r
inner join client c on c.Id=r.IdClient
where year(jour)=2017 and 1 in
    (select
    datediff(day,r.jour, r2.jour)
    from reservation r2
    where year(jour)=2017 and R2.IdClient=id
)

