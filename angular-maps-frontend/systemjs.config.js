(function(global) {
  // map tells the System loader where to look for things
  var map = {
    'app':                        'scripts',
    'rxjs':                       'node_modules/rxjs',
    '@angular':                   'node_modules/@angular',
    'angular2-google-maps':       'node_modules/angular2-google-maps'
  };
  // packages tells the System loader how to load when no filename and/or no extension
  var packages = {
    'scripts':  { main: 'main.js',  defaultExtension: 'js' },
    'rxjs':  { defaultExtension: 'js' },

	// ----> ADD THE FOLLOWING LINE !!!
    'angular2-google-maps':       { defaultExtension: 'js' }
  };
  var packageNames = [
    '@angular/common',
    '@angular/compiler',
    '@angular/core',
    '@angular/http',
    '@angular/platform-browser',
    '@angular/platform-browser-dynamic',
    '@angular/router',
    '@angular/router-deprecated',
    '@angular/testing',
    '@angular/upgrade',
  ];
  // add package entries for angular packages in the form '@angular/common': { main: 'index.js', defaultExtension: 'js' }
  packageNames.forEach(function(pkgName) {
    packages[pkgName] = { main: 'index.js', defaultExtension: 'js' };
  });
  var config = {
    map: map,
    packages: packages
  }
  System.config(config);
})(this);
