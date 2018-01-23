import './css/site.css';
import React from "react";
import { render } from 'react-dom';
import { AppContainer } from 'react-hot-loader';
import { BrowserRouter } from 'react-router-dom';
import * as RoutesModule from './routes';

let routes = RoutesModule.routes;

class Main extends React.Component {
    render() {
        const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href');
        return (<AppContainer>
            <BrowserRouter children={routes} basename={baseUrl} />
        </AppContainer>);
    }
}

render(<Main />, document.getElementById('react-app'));