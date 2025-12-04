// Simple test to verify PostgreSQL connection
const { Client } = require('pg');

const config = {
  host: '127.0.0.1',  // Use IP instead of localhost
  port: 54011,
  database: 'postgres',
  user: 'postgres',
  password: 'postgres',
  ssl: false,
  connectionTimeoutMillis: 10000,
};

console.log('Attempting to connect to PostgreSQL...');
console.log('Config:', { ...config, password: '****' });

const client = new Client(config);

client.connect()
  .then(() => {
    console.log('✅ Connected successfully!');
    return client.query('SELECT version()');
  })
  .then((result) => {
    console.log('✅ Query successful!');
    console.log('PostgreSQL version:', result.rows[0].version);
    return client.end();
  })
  .then(() => {
    console.log('✅ Connection closed');
    process.exit(0);
  })
  .catch((error) => {
    console.error('❌ Connection failed:', error.message);
    console.error('Full error:', error);
    process.exit(1);
  });
