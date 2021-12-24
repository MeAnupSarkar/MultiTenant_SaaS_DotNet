// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement.Schema;

namespace Data.Azure.Service
{
    internal class ShardingService
    {
        public ShardMapManager ShardMapManager { get; private set; }

        public ListShardMap<int> ShardMap { get; private set; }

        // Bootstrap Elastic Scale by creating a new shard map manager and a shard map on 
        // the shard map manager database if necessary.
        public ShardingService(string smmserver, string smmdatabase, string smmconnstr)
        {
            // Connection string with administrative credentials for the root database
            SqlConnectionStringBuilder connStrBldr = new SqlConnectionStringBuilder(smmconnstr);
            connStrBldr.DataSource = smmserver;
            connStrBldr.InitialCatalog = smmdatabase;

            // Deploy shard map manager.
            ShardMapManager smm;
            if (!ShardMapManagerFactory.TryGetSqlShardMapManager(connStrBldr.ConnectionString, ShardMapManagerLoadPolicy.Lazy, out smm))
            {
                this.ShardMapManager = ShardMapManagerFactory.CreateSqlShardMapManager(connStrBldr.ConnectionString);

               
            }
            else
            {
                this.ShardMapManager = smm;
            }

             

            ListShardMap<int> sm;
            if (!ShardMapManager.TryGetListShardMap<int>("SaaSElasticScale", out sm))
            {
                this.ShardMap = ShardMapManager.CreateListShardMap<int>("SaaSElasticScale");

                SchemaInfo schemaInfo = new SchemaInfo();
                // schemaInfo.Add(new ReferenceTableInfo("Products"));

                schemaInfo.Add(new ShardedTableInfo("Products", "TenantId"));
                this.ShardMapManager.GetSchemaInfoCollection().Add("SaaSElasticScale", schemaInfo);
            }
            else
            {
                this.ShardMap = sm;
            }
        }

     
        public void RegisterNewShard(string server, string database, string connstr, int key)
        {
            Shard shard;
            ShardLocation shardLocation = new ShardLocation(server, database);

            if (!this.ShardMap.TryGetShard(shardLocation, out shard))
            {
                shard = this.ShardMap.CreateShard(shardLocation);
            }

            SqlConnectionStringBuilder connStrBldr = new SqlConnectionStringBuilder(connstr);
            connStrBldr.DataSource = server;
            connStrBldr.InitialCatalog = database;

            PointMapping<int> mapping;
            if (!this.ShardMap.TryGetMappingForKey(key, out mapping))
            {
                this.ShardMap.CreatePointMapping(key, shard);
            }
        }


        public void DeleteShard(string server, string database, string connstr, int key)
        {
            Shard shard;
            ShardLocation shardLocation = new ShardLocation(server, database);

            if (!this.ShardMap.TryGetShard(shardLocation, out shard))
            {
                shard = this.ShardMap.CreateShard(shardLocation);

                if(shard != null)
                {
                    try
                    {
                        //Delete Shard
                        this.ShardMap.DeleteShard(shard);
                        

                        PointMapping<int> mapping;
                        if (!this.ShardMap.TryGetMappingForKey(key, out mapping))
                        {
                            //Delete Shard Mapping
                            this.ShardMap.DeleteMapping(mapping);
                        }

                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                   
            }

            SqlConnectionStringBuilder connStrBldr = new SqlConnectionStringBuilder(connstr);
            connStrBldr.DataSource = server;
            connStrBldr.InitialCatalog = database;
 
        }
    }
}
