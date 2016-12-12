ab -n 1000 -c 5 -g GetJwks.tsv "http://localhost:5019/jwks"
gnuplot
set terminal jpeg
set output "GetJwks.jpg"
set title "GetJwks 1000 requests - 5 concurrency"
set size 1,0.7
set grid y
set xlabel "requests"
set ylabel "response time (ms)"
plot "GetJwks.tsv" using 9 smooth sbezier with lines title "Server"