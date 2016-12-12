ab -p register-client.txt  -T application/json -c 5 -n 1000 -g RegisterClient.tsv http://localhost:5019/registration
gnuplot
set terminal jpeg
set output "RegisterClient.jpg"
set title "RegisterClient 1000 requests - 5 concurrency"
set size 1,0.7
set grid y
set xlabel "requests"
set ylabel "response time (ms)"
plot "RegisterClient.tsv" using 9 smooth sbezier with lines title "Server"