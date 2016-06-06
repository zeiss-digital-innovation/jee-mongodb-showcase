const gulp = require('gulp');

// install packages  > npm install --save-dev gulp-replace
const del = require('del');
const replace = require('gulp-replace');
const war = require('gulp-war');
const zip = require('gulp-zip');
const ts = require('gulp-typescript');
const SystemBuilder = require('systemjs-builder');

var builder = new SystemBuilder();

// clean the contents of the distribution directory
gulp.task('clean', function () {
    return del(['dist/**/*', 'scripts/**/*']);
});

// compile typescript
gulp.task('compile', ['clean'], function() {
    var project = ts.createProject('tsconfig.json');
    return project.src()
        .pipe(ts(project))
        .pipe(gulp.dest('scripts'));
});

// copy assets
gulp.task('copy:assets', ['compile'], function() {
    builder.loadConfig('./systemjs.config.js')
        .then(function() {
            return builder.buildStatic('app', 'dist/bundle.js', {
                minify: false,
                mangle: false,
                rollup: false
            });
        });

    return gulp.src([
        'node_modules/es6-shim/es6-shim.min.js',
        'node_modules/zone.js/dist/zone.js',
        'node_modules/reflect-metadata/Reflect.js',
        'node_modules/systemjs/dist/system.src.js',
        'node_modules/angular2-google-maps/core.js',
        'node_modules/angular2-google-maps/bundles/angular2-google-maps.js',
        'node_modules/jquery/dist/jquery.min.js',
        'node_modules/tether/dist/js/tether.min.js',
        'node_modules/bootstrap/dist/js/bootstrap.min.js',
        'node_modules/bootstrap/dist/css/bootstrap.min.css',
        'styles/*.css',
        'templates/**/*',
        'images/**/*',
        'assets/**/*',
        'index.html'
    ], {"base": "."})
    .pipe(gulp.dest('dist'))
});

// replace base in index to target war name
gulp.task('replace:index', ['copy:assets'], function() {
    //var config = require('./dist/scripts/config.json');
    return gulp.src('dist/index.html')
        .pipe(replace(
            '<base href="/">',
            '<base href="/campus/">'
        ))
        .pipe(replace(
            '<script src="systemjs.config.js"></script>',
            ''
        ))
        .pipe(replace(
            '<script> System.import(\'app\').catch(function (err) {console.error(err);});</script>',
            ''
        ))
        .pipe(replace(
            '</body>',
            '<script src="bundle.js"></script></body>'
        ))
        .pipe(gulp.dest('dist'));
});

// build war
gulp.task('war', ['replace:index'], function () {
    gulp.src(['dist/**/*'])
        .pipe(war({
            welcome: 'index.html',
            displayName: 'Saxonia Campus MongoDB Showcase Angular2 Frontend'
        }))
        .pipe(zip('angular-maps-frontend.war'))
        .pipe(gulp.dest('dist'));
});

gulp.task('default', ['war']);
