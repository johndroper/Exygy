import 'bootstrap/dist/css/bootstrap.css';
import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter } from 'react-router-dom';
import App from './App';
import registerServiceWorker from './registerServiceWorker';

const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href');
const rootElement = document.getElementById('root');

ReactDOM.render(
  <BrowserRouter basename={baseUrl}>
    <App />
  </BrowserRouter>,
  rootElement);

if (window.location.href.includes('index'))
    registerServiceWorker();
else
    unregisterAndReload();

function unregisterAndReload() {
    navigator.serviceWorker.ready.then(registration => {
        registration.unregister().then(() => {
            window.location.reload();
        });
    });
}

