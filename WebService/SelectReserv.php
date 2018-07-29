<?php
/* DA SILVA Thomas, 06/03/18 : WebService Projet Lifi */

/* Lien vers fichier de connexion */
include_once("Connect.php");

/* Requête principale */
$query = $connect->query("SELECT ID, Reference, Debut_Reservation, Fin_Reservation, Arrivee, Depart, Place_ID, Client_ID FROM reservation WHERE Client_ID = '" . $_GET["Client_ID"] . "'");

$data = array();
while ($r = mysqli_fetch_assoc($query))
{
	$data[] = array("ID" => $r["ID"], "Reference" => $r["Reference"], "Debut_Reservation" => $r["Debut_Reservation"], "Fin_Reservation" => $r["Fin_Reservation"], "Arrivee" => $r["Arrivee"], "Depart" => $r["Depart"], "Place_ID" => $r["Place_ID"], "Client_ID" => $r["Client_ID"]);
}

header("content-type: application/json");
if (empty($data))
{
	echo "Aucun résultat"; 
} 

else
{
	echo json_encode($data);
}

/* Ferme la connexion */
@mysqli_close($connect);
?>