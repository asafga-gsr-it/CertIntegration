# Cookbook Name:: zabbix_agent
# Recipe:: default
# Recipe to install and configure Zabbix agent


gcc=node['common']['yum']['gcc']
curl=node['common']['yum']['curl']
jannson=node['common']['yum']['jansson']
caPath=node['common']['ca']['url']
causer=node['common']['ca']['user']
confFile=node['common']['ca']['conffile']



######### Install z package ###########
package gcc do
   action :install
end


package  curl do
  action :install
end

package jannson do
  action :install
end


######### Create CaPath Directory #############
directory caPath do
  owner causer
  mode '0755'
  action :create
end

######### Give Causer Read Premmisions for root Directory #############
directory '/sys/devices/virtual/dmi/id/product_uuid' do
  owner causer
   mode '0755' 
end

######### Give Causer Read Premmisions for root Directory #############
directory '/dev/xvda1' do
  owner causer
  mode '0755'
end


######### Give ca conf file premmisions #############
file confFile do
  owner causer
  mode '0755'
end

