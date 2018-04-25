import './css/site.css';

import React from "react";
import { render } from 'react-dom';
import { AppContainer } from 'react-hot-loader';
import { BrowserRouter } from 'react-router-dom';
import { I18nextProvider } from 'react-i18next';
import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';
import i18n from './i18n';
import * as RoutesModule from './routes';

let routes = RoutesModule.routes;

class Main extends React.Component {
    render() {
        const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href');
        return (
                <AppContainer>
                    <I18nextProvider
                        i18n={i18n}
                        initialI18nStore={window.initialI18nStore}
                        initialLanguage={window.initialLanguage}
                    >
                        <BrowserRouter children={routes} basename={baseUrl} />
                    </I18nextProvider>
                </AppContainer>);
    }
}

render(<Main />, document.getElementById('react-app'));