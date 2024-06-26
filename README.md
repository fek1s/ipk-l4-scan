# Project 2 - OMEGA: L4 Scanner
### Autor: Jakub Fukala (xfukal01)

## Obsah

1. [**Úvod**](#úvod)
2. [**Teorie skenování portů**](#teorie-skenování-portů)
3. [**Spuštění**](#spuštění)
4. [**Implementovaná funkcionalita**](#implementovaná-funkcionalita)
5. [**Testování**](#testování)
6. [**Návratové kódy**](#návratové-kódy)
7. [**Zdroje**](#zdroje)

## Úvod
Cílem projektu bylo vytvořit síťový skener, který bude schopen skenovat cílový počítač pomocí různých protokolů. 
V mém případě jsem se zaměřil na skenování pomocí protokolu TCP a UDP. Program je schopen skenovat cílový počítač z 
jednoho rozhraní a zobrazit informace o dostupných portech. Program je napsán v jazyce C#.

## Teorie skenování portů

### UDP skenování
UDP je protokol, který je založen na principu posílání datových paketů bez navázání spojení.
Pro skenování pomocí UDP je potřeba vytvořit soket, který bude schopen posílat data pomocí UDP protokolu.
Při skenování je potřeba poslat datový paket na cílový port a čekat na odpověď. Pokud je odpovědí packet 
ICMP s typem 3 a kódem 3, tak je port uzavřený. Pokud žádná odpověď nepřijde, tak je port otevřený.

### TCP skenování
TCP je spojovaný protokol, který používá třícestné potřesení rukou pro navázání spojení.
Pro skenování pomocí TCP je nutné vytvořit spojení s cílovým portem a zpracovat odpověď. Existují různé typy TCP skenování:
- **SYN skenování**: Posílá SYN packet na cílový port. Pokud je odpověď SYN-ACK, je port otevřený. Pokud je odpověď RST, je port uzavřený.
- **FIN skenování**: Posílá FIN packet na cílový port. Pokud je odpověď RST, je port otevřený. Pokud je odpověď FIN-ACK, je port uzavřený.
- **NULL skenování**: Neposílá žádný speciální packet. Pokud je odpověď RST, je port otevřený. Pokud je odpověď SYN-ACK nebo nic, je port uzavřený.

## Spuštění

Program je možné spustit pomocí příkazové řádky s následujícími argumenty:
- `-i <rozhraní>`/ `--interface <rozhraní> ` - specifikuje rozhraní, ze kterého bude program skenovat
- `-u <porty>`/ `--pu <porty>` - specifikuje porty, které budou skenovány pomocí UDP protokolu
- `-t <porty>`/ `--pt <porty>` - specifikuje porty, které budou skenovány pomocí TCP protokolu
- `-w <timeout>`/ `--wait <timeout>` - specifikuje timeout pro čekání na odpověď (v ms) (defaultně 5000ms)
- `-r` /  - specifikuje, počet pokusů o opětovné odeslání packetu při neobdržení odpovědi během UDP skenování (defaultně 1)
- `-d` / `--debug` - specifikuje, zda se mají zobrazovat ladící informace
- Dále musí být zadán cíl skenování čili **IP adresa** cíle nebo **doménové jméno**

### Formát portů
- Porty mohou být zadány jako jednotlivé čísla nebo rozsahy oddělené pomlčkou (např. 22-80)
- Porty mohou být odděleny čárkou (např. 22,80,443)



## Implementovaná funkcionalita

- Program je schopen skenovat cílový počítač pomocí protokolu TCP a UDP pomocí zadaného rozhraní a 
zobrazovat informace o dostupných portech.
- Pro získání informací o cíli je využita třída `Dns` a `IPAddress` z knihovny `System.Net` pro získání IP adresy cíle.
- Získání informací o lokálním rozhraní je implementováno pomocí třídy `NetworkInterface` z knihovny `System.Net.NetworkInformation`.


### UDP

- Pro UDP skenování je v programu implementována třída `UpdPortScanner`, která poskytuje metodu `Scan` k provádění 
skenování cílového portu. Tato třída využívá jak třídu `Socket` pro příjem ICMP zpráv, tak třídu `UdpClient` pro 
odesílání UDP paketů.

- Metoda `Scan` postupně zasílá prázdný UDP paket na zvolený port cílového zařízení. Poté čeká na odpověď v podobě ICMP 
zprávy. Pokud přijde ICMP zpráva s typem 3 a kódem 3, port je považován za uzavřený a skenování končí s výsledkem`UdpPortScanResult.Closed`.

- V případě, že nebyla přijata žádná ICMP zpráva a vypršel nastavený timeout, je port považován za otevřený a 
skenování končí s výsledkem `UdpPortScanResult.Open`.

- Pro IPv6 adresy je implementována dodatečná kontrola, která kontroluje, zda příchozí ICMP zpráva neindikuje uzavřený port.

- Pokud nastane jakákoliv jiná chyba během skenování, je výsledek skenování označen jako `UdpPortScanResult.Error`.

Tato implementace umožňuje spolehlivé skenování UDP portů s ohledem na různé reakce a situace, které mohou nastat 
při komunikaci s cílovým zařízením.

### TCP

- Implementace TCP skenování využívá třídu `TcpPortScanner` a metodu `Scan`, která provádí skenování cílového portu. 
Při skenování se vytvoří instance třídy `TcpClient`, která umožňuje vytvoření TCP spojení se zvoleným cílovým zařízením a portem.

- Metoda `Scan` začíná spojení na zvoleném portu pomocí asynchronní metody `BeginConnect`, která zahajuje inicializaci 
spojení bez blokování hlavního vlákna. Počká na dokončení pokusu o spojení pomocí metody `EndConnect`.

- Pokud se spojení podaří navázat, je považován daný port za otevřený, a výsledek skenování je vrácen 
jako `TcpPortScanResult.Open`.

- V případě, že spojení nebylo úspěšné a došlo k chybě spojení (například odpověďí RST), je port považován za 
uzavřený, a výsledek skenování je vrácen jako `TcpPortScanResult.Closed`.

- V případě jakékoliv jiné chyby, která není přímo spojena s uzavřením portu (například selhání spojení z 
jiného důvodu), je výsledek skenování vrácen jako `TcpPortScanResult.Error`.



## Testování
- Testování probíhalo mezi počítači s operačními systémy Fedora 39 a virtualizovaným Ubuntu 64-bit.
- Skenovaní probíhalo směrem z Fedory na Ubuntu.
- Pro testování byly využity nástroje `nc` a `ncat` pro vytvoření testovacích serverů.
- Funkčnost programu byla otestována pomocí nástroje Wireshark, který zachycuje síťový provoz.
- V rámci testovaní je program spoušten s parametrem `-d`, který zobrazuje ladící informace.

### UDP skenování
#### Skenování otevřeného portu

- Spouštěno : `ipk-l4-scan -i wlo1 -u 2000 10.102.90.139 -w 2000 -d`
- Výstup:
```
Interface: wlo1
Target: 10.102.90.139
Timeout: 2000
TCP ports:
UDP ports: 2000
Interface IP: 10.102.90.141
Interface IPv6: fe80::dd3:b0a0:cfc9:d1b%3
Retransmission count: 1
Host resolved as: 10.102.90.139
==============================================
2000/udp: Open
```
- Zde je výstřižek z Wiresharku zobrazující tuto komunikaci:
- Vydíme, že je zaslán UDP packet na port 2000 a jelikož nebyla přijata žádná odpověď, tak je zaslán další packet, 
pokud ani na něj nepřijde odpověď, je port považován za otevřený.
![doc/udpscanopen.png](doc/udpscanopen.png)

#### Skenování uzavřeného portu
- Spouštěno : `ipk-l4-scan -i wlo1 -u 2000 10.102.90.139 -w 2000 -d`
- Výstup:
```
Interface: wlo1
Target: 10.102.90.139
Timeout: 2000
TCP ports: 
UDP ports: 2000
Interface IP: 10.102.90.141
Interface IPv6: fe80::dd3:b0a0:cfc9:d1b%3
Retransmission count: 1
Host resolved as: 10.102.90.139 
==============================================
2000/udp: Closed
```
- Zde je výstřižek z Wiresharku zobrazující tuto komunikaci:
- Vydíme, že je zaslán UDP packet na port 2000 a jelikož odpověď je ICMP zpráva s typem 3 a kódem 3, je port považován za uzavřený.
![doc/udpscanclosed.png](doc/udp_port_closed.png)

#### Skenovaní rozsahu portů
- Spuštěno: `ipk-l4-scan -i wlo1 -u 1999-2001 10.102.90.139 -w 2000 -d`
- Výstup:
```
Interface: wlo1
Target: 10.102.90.139
Timeout: 2000
TCP ports: 
UDP ports: 1999, 2000, 2001
Interface IP: 10.102.90.141
Interface IPv6: fe80::dd3:b0a0:cfc9:d1b%3
Retransmission count: 1
Host resolved as: 10.102.90.139 
==============================================
1999/udp: Closed
2000/udp: Open
2001/udp: Closed
```
- Výstřižek z Wiresharku:
![doc/udprange.png](doc/udp_rangescan.png)

#### Skenování seznamu portů
- Spuštěno: `ipk-l4-scan -i wlo1 -u 2000,2003,2111 10.102.90.139 -w 2000 -d`
- Výstup:
```
Interface: wlo1
Target: 10.102.90.139
Timeout: 2000
TCP ports: 
UDP ports: 2000, 2003, 2111
Interface IP: 10.102.90.141
Interface IPv6: fe80::dd3:b0a0:cfc9:d1b%3
Retransmission count: 1
Host resolved as: 10.102.90.139 
==============================================
2000/udp: Open
2003/udp: Closed
2111/udp: Closed
```

- Výstřižek z Wiresharku:
- ![doc/udpportlist.png](doc/udp_listscan.png)

#### Skenevání IPv6 adresy
- Spuštěno: `ipk-l4-scan -i wlo1 -u  5000,2000 2a0d:187:204f:e000:cc34:aed9:33c5:379e  -w 2000 -d`
- Ubuntu spouštěno: `ncat -l -v  -u 2a0d:187:204f:e000:cc34:aed9:33c5:379e -p 2000`
- Výstup:
```
Interface: wlo1
Target: 2a0d:187:204f:e000:cc34:aed9:33c5:379e
Timeout: 2000
TCP ports: 
UDP ports: 5000, 2000
Interface IP: 192.168.1.109
Interface IPv6: 2a0d:187:204f:e000:c4a4:bae:7ec0:d9fe
Retransmission count: 1
Target resolved as:
2a0d:187:204f:e000:cc34:aed9:33c5:379e
==============================================
5000/udp: Closed
2000/udp: Open
```
- Výstřižek z Wiresharku:
- Zde je vidět, že je zaslán UDP packet na port 5000 a odpověď na něj ICMPv6 s kódem 4, což značí, že port je uzavřený.
- Dále je zaslán UDP packet na port 2000 a jelikož nebyla přijata žádná odpověď, je port považován za otevřený.
![doc/udpipv6.png](doc/udp_ipv6_scan.png)

### TCP skenování
#### Skenování otevřeného portu
- Ubuntu spouštěno: `nc -4 -l -v 10.102.90.139 2000`   
- Fedora spouštěno: `ipk-l4-scan -i wlo1 -t 2000 10.102.90.139 -w 2000 -d`
- Výstup:
```
Interface: wlo1
Target: 10.102.90.139
Timeout: 2000
TCP ports: 2000
UDP ports: 
Interface IP: 10.102.90.141
Interface IPv6: fe80::dd3:b0a0:cfc9:d1b%3
Retransmission count: 1
Host resolved as: 10.102.90.139 
==============================================
2000/tcp: Open
```
- Výstřižek z Wiresharku:
- Zde je vidět, že je zaslán TCP packet na port 2000 a jelikož je odpověď SYN-ACK, je port považován za otevřený. Dále je 
pak spojení co nejrychleji ukončeno.
- ![doc/tcpscanopen.png](doc/tcp_portopen.png)

#### Skenování uzavřeného portu
- Spuštěno: `ipk-l4-scan -i wlo1 -t 2001 192.168.1.193 -w 2000 -d`
- Výstup:
```
Interface: wlo1
Target: 192.168.1.193
Timeout: 2000
TCP ports: 2001
UDP ports: 
Interface IP: 192.168.1.109
Interface IPv6: 2a0d:187:204f:e000:c4a4:bae:7ec0:d9fe
Retransmission count: 1
Target resolved as:
192.168.1.193
==============================================
2001/tcp: Closed
```
- Výstřižek z Wiresharku:
- Zde je vidět, že je zaslán TCP packet na port 2001 a jelikož je odpověď RST, je port považován za uzavřený.
- ![doc/tcpscanclosed.png](doc/tcp_portclosed.png)



#### Skenovaní IPv6 adresy
- Spuštěno: `ipk-l4-scan -i wlo1 -t  5000,2000 2a0d:187:204f:e000:cc34:aed9:33c5:379e -w 2000 -d`
- Ubuntu spouštěno: `ncat -l -v 2a0d:187:204f:e000:cc34:aed9:33c5:379e -p 2000`
- Výstup:
```
Interface: wlo1
Target: 2a0d:187:204f:e000:cc34:aed9:33c5:379e
Timeout: 2000
TCP ports: 5000, 2000
UDP ports: 
Interface IP: 192.168.1.109
Interface IPv6: 2a0d:187:204f:e000:c4a4:bae:7ec0:d9fe
Retransmission count: 1
Target resolved as:
2a0d:187:204f:e000:cc34:aed9:33c5:379e
==============================================
5000/tcp: Closed
2000/tcp: Open
```
- Výstřižek z Wiresharku:
- ![doc/tcpipv6.png](doc/tcp_ipv6_scan.png)

#### SYN skenování
- Spouštěno : `ipk-l4-scan -i wlo1 -t 2000 192.168.1.193 -w 2000 -d -ts`
- Výstup:
```
Interface: wlo1
Target: 192.168.1.193
Timeout: 2000
TCP ports: 2000
UDP ports: 
Interface IP: 192.168.1.109
Interface IPv6: 2a0d:187:204f:e000:c4a4:bae:7ec0:d9fe
Retransmission count: 1
Target resolved as:
192.168.1.193
==============================================
2000/tcp: Open
```
- Výstřižek z Wiresharku:
- Zde vydíme zaslání SYN packetu na port 2000, ale jelikož nedostane odpověď, tak není možné určit stav portu.
- ![doc/synscan.png](doc/tcp_synscan.png)

### Testování argumentů
- Provádí zakladní testovaní zpracování argumentů
- Spouštěno `ipk-l4-scan -i lo --test`
```
TestInterface: Passed
TestInvalidInterface: Passed
UnspecifiedInterface: Passed
TestInvalidPortRange: Passed
TestPortRangeList: Passed
TestPortRangeMissingSecondPort: Passed
TestPortRangeNegative: Passed
TestPortRangeInvalidFormat: Passed
TestPortRangeNotNumber: Passed
```


## Návratové kódy
- `0` - vše proběhlo v pořádku
- `1` - Zadané rozhraní není aktivní nebo neexistuje např. l0 namísto lo
- `2` - Špatný format argumentu rozhraní (např. -i <nic>)
- `3` - Špatný formát argumentu portu (např. -u 22, , -u 22- , -u 22,-2, -u y-x)
- `4` - Nebyly poskytnuty nezbytné argumenty
- `5` - Cíl nedostupný z daného rozhraní
- `6` - Lokální rozhraní nemá přirazenou IPv6 adresu

## Zdroje
- [Získání informací o aktivních rozhraních](https://learn.microsoft.com/en-us/dotnet/api/system.net.networkinformation?view=net-8.0)
- [gitignore](https://github.com/github/gitignore/blob/main/VisualStudio.gitignore)
- [Typy TCP skenoání](https://nmap.org/book/man-port-scanning-techniques.html)
### Získaní informací o cíli
- [Dns Class](https://learn.microsoft.com/en-us/dotnet/api/system.net.dns?view=net-8.0)
- [IPaddress Class](https://learn.microsoft.com/en-us/dotnet/api/system.net.ipaddress?view=net-8.0)
### Sokety
- [Raw Sokets](https://www.opensourceforu.com/2015/03/a-guide-to-using-raw-sockets/)
- [TCP](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services)