<?php
/* DA SILVA Thomas, 06/03/18 : WebService Projet Lifi */

/* Variables nécéssaires pour se connecter à la bdd */
/* En local */
/*$AdresseBDD = 'localhost';
$User = 'root';
$Password = 'root';
$BDDName = 'smartparkinglifi_2017-2018_v1.0';*/

/* En ligne, serveur 1 */
$AdresseBDD = 'mysql-projetlifi.alwaysdata.net';
$User = '156739';
$Password = 'projetlifi';
$BDDName = 'projetlifi_bdd';

/* En ligne, serveur 2 */
/*$AdresseBDD = 'localhost';
$User = 'id5870247_156739';
$Password = 'projetlifi';
$BDDName = 'id5870247_projetlifi_bdd';*/

/* Requête de connexion */
$connect = new mysqli($AdresseBDD, $User, $Password, $BDDName) or die(mysqli_error())
?>