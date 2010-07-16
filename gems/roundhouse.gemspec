version = File.read(File.expand_path("../VERSION",__FILE__)).strip

Gem::Specification.new do |s|
  #s.platform    = 'mswin32'
  s.platform    = Gem::Platform::RUBY
  s.name        = 'roundhouse'
  s.version     = version
  s.files = Dir['lib/**/*'] + Dir['bin/**/*']
  s.bindir = 'bin'
  s.executables << 'rh'
  
  s.summary     = 'RoundhousE - Professional Database Change and Versioning Management'
  s.description = 'RoundhousE is a Professional Database Change and Versioning Management tool'
  
  s.authors           = ['Rob "FerventCoder" Reynolds','Pascal Mestdach','Jochen Jonckheere','Dru Sellers']
  s.email             = 'chucknorrisframework@googlegroups.com'
  s.homepage          = 'http://projectroundhouse.org'
  s.rubyforge_project = 'roundhouse'
end