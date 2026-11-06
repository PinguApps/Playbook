#!/bin/sh
set -eu
# Run only inside the disposable test container. No host credentials or ports.
export DEBIAN_FRONTEND=noninteractive
apt-get update -qq
apt-get install -y -qq krb5-kdc krb5-admin-server krb5-user >/tmp/gannet-packages.log
mkdir -p /gannet
cat >/gannet/krb5.conf <<'EOF'
[libdefaults]
 default_realm = GANNET.TEST
 dns_lookup_kdc = false
 dns_lookup_realm = false
 rdns = false
[realms]
 GANNET.TEST = {
  kdc = 127.0.0.1:61088
 }
EOF
cat >/gannet/kdc.conf <<'EOF'
[kdcdefaults]
 kdc_ports = 61088
 kdc_tcp_ports = 61088
[realms]
 GANNET.TEST = {
  database_name = /gannet/principal
  key_stash_file = /gannet/stash
  max_life = 1h
  max_renewable_life = 1h
 }
EOF
export KRB5_CONFIG=/gannet/krb5.conf KRB5_KDC_PROFILE=/gannet/kdc.conf
umask 077
master=$(openssl rand -hex 32)
kdb5_util create -s -P "$master" >/gannet/create.log 2>&1
unset master
kadmin.local -q 'addprinc -randkey surveyor@GANNET.TEST' >/gannet/user.log 2>&1
kadmin.local -q 'addprinc -randkey HTTP/station.gannet.test@GANNET.TEST' >/gannet/service.log 2>&1
kadmin.local -q 'ktadd -k /gannet/client.keytab surveyor@GANNET.TEST' >/gannet/client-key.log 2>&1
kadmin.local -q 'ktadd -k /gannet/server.keytab HTTP/station.gannet.test@GANNET.TEST' >/gannet/server-key.log 2>&1
krb5kdc -n >/gannet/kdc.log 2>&1 &
export KRB5CCNAME=FILE:/gannet/ccache
# KDC readiness is established by the actual credential acquisition.
i=0
until kinit -kt /gannet/client.keytab surveyor@GANNET.TEST; do
 i=$((i+1)); test "$i" -lt 20; sleep .2
done
dpkg-query -W krb5-kdc libgssapi-krb5-2 libkrb5-3 openssl
