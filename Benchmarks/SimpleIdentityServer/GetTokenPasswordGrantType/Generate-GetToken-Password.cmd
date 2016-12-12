ab -p get-token-password.txt  -T application/x-www-form-urlencoded -c 5 -n 1000 -g GetTokenPasswordGrantType.tsv http://localhost:5019/token
gnuplot
set terminal jpeg
set output "GetTokenPasswordGrantType.jpg"
set title "GetTokenPasswordGrantType 1000 requests - 5 concurrency"
set size 1,0.7
set grid y
set xlabel "requests"
set ylabel "response time (ms)"
plot "GetTokenPasswordGrantType.tsv" using 9 smooth sbezier with lines title "Server"