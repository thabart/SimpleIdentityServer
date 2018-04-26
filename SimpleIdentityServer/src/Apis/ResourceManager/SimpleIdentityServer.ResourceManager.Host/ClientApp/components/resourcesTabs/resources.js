import React, { Component } from "react";
import { translate } from 'react-i18next';

class Resources extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        var self = this;
        const { t } = self.props;
        return (<div className="card">
                <div className="header">
                    <h4 style={{display: "inline-block"}}>{t('resources')}</h4>
                </div>
                <div className="body">
                </div>
        </div>);
    }
}

export default translate('common', { wait: process && !process.release })(Resources);