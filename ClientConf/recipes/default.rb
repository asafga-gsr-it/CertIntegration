# Cookbook Name:: zabbix_agent
# Recipe:: default
# Recipe to install and configure Zabbix agent


platform = node['platform']
rel=node['platform_version']

########## Get OS information  #############
case platform
      when 'redhat'
         os=node['platform']
         if ( rel >= '7')
            distro=7
         end
         if ( rel >= '6.' && rel < '7' )
            distro=6
         end
      when 'suse'
         os=node['platform']
         if ( rel === '11.3' )
            distro=11.3
         end
         if ( rel === '11.4' )
            distro=11.4
         end
         if ( rel === '12.0' )
            distro=12
         end
         if ( rel === '12.1' )
            distro=12.1
         end
         if ( rel === '12.2' )
            distro=12.2
         end
	 if ( rel === '12.3' )
            distro=12.3
         end
      when 'ubuntu'
         os=node['platform']
         if ( rel >= '14.04' && rel < '16.' )
            distro=14.04
         end
	 if ( rel >= '16.04' )
            distro=16.04
         end
      when 'debian'
	 puts "Debian is not supported....."
	 return
end

#################  Set repository for current distribution  #################
if ( platform === 'suse' )
        repo_name="zabbix_repo"
        template '/etc/zypp/repos.d/zabbix.repo' do
           source 'zabbix.rpm.repo.erb'
           variables( :os => "#{os}", :distro => "#{distro}", :repo_name => "#{repo_name}" )
           owner "root"
           group "root"
           mode '644'
        end
end

if ( platform === 'redhat' )
        repo_name="zabbix_repo"
        template '/etc/yum.repos.d/zabbix.repo' do
            source 'zabbix.rpm.repo.erb'
            variables( :os => "#{os}", :distro => "#{distro}", :repo_name => "#{repo_name}" )
            owner "root"
            group "root"
            mode '644'
         end
end


if ( platform === 'debian' || platform === 'ubuntu' )
         repo_name="zabbix_repo"
         template '/etc/apt/sources.list.d/zabbix.list' do
            source 'zabbix.deb.repo.erb'
            variables( :os => "#{os}", :distro => "#{distro}", :repo_name => "#{repo_name}" )
            owner "root"
            group "root"
            mode '644'
         end
end

######### Install zabbix-agent package ###########
package 'zabbix-agent' do
   action :install
end


################# Chmod /var/run/zabbix/  #################
if ( platform === 'suse' )
   directory '/var/run/zabbix/' do
       owner 'zabbix'
       group 'zabbix'
       mode '0775'
    end
end

######### Configure agent to connect zabbix server #############
directory '/etc/zabbix/zabbix_agentd.d' do
  owner 'root'
  group 'root'
  mode '0755'
  action :create
end

directory '/var/log/zabbix/' do
  owner 'zabbix'
  group 'zabbix'
  mode '0755'
  action :create
end

directory '/var/run/zabbix/' do
  owner 'zabbix'
  group 'zabbix'
  mode '0755'
  action :create
end

cookbook_file '/etc/zabbix/zabbix_agentd.conf' do
  source 'zabbix_agentd.conf'
  owner 'root'
  group 'root'
  mode '0644'
  action :create
end


link '/etc/zabbix/zabbix-agentd.conf' do
  to '/etc/zabbix/zabbix_agentd.conf'
  link_type :symbolic
end

link '/etc/zabbix/zabbix-agent.conf' do
  to '/etc/zabbix/zabbix_agentd.conf'
  link_type :symbolic
end

link '/etc/zabbix/zabbix_agent.conf' do
  to '/etc/zabbix/zabbix_agentd.conf'
  link_type :symbolic
end

###########  Enable and Start zabbix service  ###############
case node['platform']
  when 'suse'
 	service 'zabbix-agentd' do
  	action [ :enable, :start ]
	end
  when 'redhat' || 'ubuntu' 
	service 'zabbix-agent' do 
	  action [ :enable, :start ]
	end
end
