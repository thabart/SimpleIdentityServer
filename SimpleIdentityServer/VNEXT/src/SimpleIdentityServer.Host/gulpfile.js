/// <binding Clean='clean' />
"use strict";
var gulp = require("gulp"),
  rimraf = require("rimraf"),
  concat = require("gulp-concat"),
  cssmin = require("gulp-cssmin"),
  uglify = require("gulp-uglify"),
  project = require("./project.json"),
  dnx = require("gulp-dnx"),
  runSequence = require("run-sequence"),
  liveReload = require("gulp-livereload");

var paths = {
    webroot: "./wwwroot/",
};

paths.scripts = [
    paths.webroot + "lib/jquery/dist/jquery.js",
    paths.webroot + "lib/bootstrap/dist/js/bootstrap.js"
];
paths.styles = [
    paths.webroot + "lib/bootstrap/dist/css/bootstrap.css",
    paths.webroot + "lib/bootstrap/dist/css/bootstrap-theme.css",
    paths.webroot + "css/*.css"
];
paths.concatJsDest = paths.webroot + "output/site.min.js";
paths.concatCssDest = paths.webroot + "output/site.min.css";

// Clean the code
gulp.task("clean:js", function(cb) {
    rimraf(paths.concatJsDest, cb);
});
gulp.task("clean:css", function(cb) {
    rimraf(paths.concatCssDest, cb);
});
gulp.task("clean", ["clean:js", "clean:css"]);

// Concatenate and minify the codes
gulp.task("min:js", function() {    
  gulp.src(paths.scripts, {
      base: "."
    })
    .pipe(concat(paths.concatJsDest))
    .pipe(uglify())
    .pipe(gulp.dest("."));
});
gulp.task("min:css", function() {
  gulp.src(paths.styles)
    .pipe(concat(paths.concatCssDest))
    .pipe(cssmin())
    .pipe(gulp.dest("."));
});
gulp.task("min", ["min:js", "min:css"]);

// Watch the changes and launch the minification
gulp.task('watch', function() {
    gulp.watch(paths.scripts, [ 'min:js' ]);
    gulp.watch(paths.styles, [ 'min:css' ]);
});

// Launch the application
gulp.task("dnx-run", dnx("web"));
gulp.task("dnx-watch", dnx("watch"));


// Default action
liveReload({ start: true })
gulp.task('default', function(cb) {
  return runSequence('clean', 'min', ['watch', 'dnx-watch'], cb);
});