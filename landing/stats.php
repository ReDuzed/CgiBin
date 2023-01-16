<?php 
    $maxstats = 15;
    $week = '';
    $cmd = "--total";
    $cmd_r = '';
    $faction = ' ';
    $bg = '';
    $meter = '--meter';
    if (isset($_POST['stats'])) {
        $maxstats = $_POST['stats'];
    }
    if (isset($_POST['faction'])) {
        $faction = $_POST['faction'];
    }
    if (isset($_POST['username'])) {
        $uname = $_POST['username'];
    }
    if (isset($_POST['week'])) {
        $week = '--week';
    }
    if (isset($_POST['cep'])) {
        $meter = '';
    }
    if (isset($_POST['background'])) {
        $bg = '--bgflag';
    }
    if (strlen($faction) == 2 && ($faction == 'tr' || $faction == 'nc' || $faction == 'vs'))
    {
        $cmd = '--' . $faction;
    }
    if (isset($uname) && strlen($uname) > 0) 
    {
        $cmd = "--player";
        $cmd_r = $uname;
    }
    //$path = 'C:\xampp\htdocs\http\landing\bin\\';
    //$command = $path . "cmd.exe /c ". $path . 'CgiBin.exe --maxstats ' . $maxstats . ' --bgflag --apiurl http://www.tserver.online:8080/api/char_stats_cep/0 --dbname .\bin\weekly_stats --texture .\Resources\texture.png --output .\Resources\_result.png ' . $cmd . ' ' . $cmd_r;
    $_rand = rand(0, 9999);
    $args = "--maxstats $maxstats $bg --rand $_rand $meter $week $cmd $cmd_r";
    
    $sock = socket_create(AF_INET, SOCK_STREAM, SOL_TCP);
    socket_connect($sock, 'localhost', 9876);
    socket_write($sock, $args, strlen($args));
//  socket_recv($sock, $data, 10200100, MSG_WAITALL);
    socket_close($sock);
    while (!file_exists(".\output\_result_$_rand.png"))
    {
        sleep(3);
    }
?>
<!DOCTYPE html>

<html lang="en-us" xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8" />
    <title>Details</title>
    <link rel="stylesheet" type="text/css" href="styles.css" />
</head>
<body>
    <div class="stats">
        <?php
            echo "<img src='.\output\_result_$_rand.png' />";
        ?>
    </div>
</body>
</html>
