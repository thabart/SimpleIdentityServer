ab -n 1000 -c 5 -g WellKnownConfiguration.tsv "http://localhost:5000/.well-known/openid-configuration"
gnuplot
set terminal jpeg
set output "WellKnownConfiguration.jpg"
set title "WellKnownConfiguration 1000 requests - 5 concurrency"
set size 1,0.7
set grid y
set xlabel "requests"
set ylabel "response time (ms)"
plot "WellKnownConfiguration.tsv" using 9 smooth sbezier with lines title "Server"