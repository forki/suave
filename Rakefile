require 'bundler/setup'

require 'albacore'
require 'albacore/nuget_model'
require 'albacore/tasks/versionizer'
require 'albacore/task_types/nugets_pack'
require 'albacore/task_types/asmver'
require 'albacore/ext/teamcity'

require 'xsemver'

Albacore::Tasks::Versionizer.new :versioning

include ::Albacore::NugetsPack

nugets_restore :restore do |p|
  p.out = 'packages'
  p.exe = 'buildsupport/NuGet.exe'
end

desc "Perform full build"
task :build => [:versioning, :restore, :build_quick]

build :build_quick do |b|
  b.file = 'suave.sln'
  b.prop 'Configuration', 'Release'
end

directory 'build/pkg'

desc "Create a nuget for Suave"
task :create_nuget => [ 'build/pkg', :versioning, :build] do
  p = Albacore::NugetModel::Package.new

  p.with_metadata do |m|
    m.id            = "Suave"
    m.version       = ENV['NUGET_VERSION']
    m.authors       = 'Ademar Gonzalez'
    m.description   = 'Suave is a simple web development F# library providing a lightweight web server and a set of combinators to manipulate route flow and task composition.'
    m.language      = 'en-GB'
    m.copyright     = 'Ademar Gonzalez'
    m.release_notes = "Full version: #{ENV['BUILD_VERSION']}."
    m.license_url   = "https://github.com/ademar/suave/blob/master/COPYING"
    m.project_url   = "http://suave.io"
  end

  p.add_file  "Suave/bin/Release/suave.dll",    'lib/net45'
  p.add_file  "Suave/bin/Release/suave.xml",    'lib/net45'

  p.add_file  "libs/ManagedOpenSsl.dll",        'lib'
  p.add_file  "libs/ManagedOpenSsl.xml",        'lib'
  p.add_file  "libs/ManagedOpenSsl.dll.config", 'build/native'
  p.add_file  "libs/libeay32.dll",              'build/native'
  p.add_file  "libs/ssleay32.dll",              'build/native'
  p.add_file  "libs/libcrypto.so.1.0.0",        'build/native'
  p.add_file  "libs/libssl.so.1.0.0",           'build/native'
  p.add_file  "libs/libcrypto.1.0.0.dylib",     'build/native'
  p.add_file  "libs/libssl.1.0.0.dylib",        'build/native'
  p.add_file  "buildsupport/suave.targets",     'build'

  File.write 'suave.nuspec', p.to_xml

  cmd = Albacore::NugetsPack::Cmd.new 'buildsupport/NuGet.exe',
                                      out: "build/pkg"

  pkg, spkg = cmd.execute 'suave.nuspec'
  publish_artifact 'suave.nuspec', pkg
end

def publish_artifact nuspec, nuget
  Albacore.publish :artifact, OpenStruct.new(
    :nuspec   => nuspec,
    :nupkg    => nuget,
    :location => nuget
  )
end

desc "Create the assembly info file"
asmver :assembly_info => :versioning do |x|
  x.attributes assembly_version: ENV['FORMAL_VERSION'],
    assembly_product: 'Suave.IO',
    assembly_title: 'Suave.IO Framework',
    assembly_description: 'Suave is an F# library providing a lightweight web server and a set of combinators to manipulate route flow and task composition.',
    assembly_copyright: '(c) 2013 by Ademar Gonzalez',
    auto_open: 'Suave.Utils'

  x.file_path = 'Suave/AssemblyInfo.fs'
  x.namespace = "Suave"
end

task :increase_version_number do
  # inc patch version in .semver
  s = SemVer.find
  s.patch += 1
  s.save
  ENV['NUGET_VERSION'] = s.format("%M.%m.%p%s")
end

desc 'release the next version'
task :release_next => [ :increase_version_number, :assembly_info , :create_nuget ] do
  s = SemVer.find.format("%M.%m.%p%s")
  # commit and tag
  system %q[git add .semver]
  system %q[git add Suave/AssemblyInfo.fs]
  system "git commit -m \"released v#{s.to_s}\""
  system "mono buildsupport/NuGet.exe setApiKey #{ENV['NUGET_KEY']}"
  system "mono buildsupport/NuGet.exe push build/pkg/suave.#{s.to_s}.nupkg"
end

task :default => :create_nuget
