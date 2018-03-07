import React, { Component } from "react";
import { translate } from 'react-i18next';

class Cache extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        var self = this;
        const { t } = self.props;
        return (<div>
            <h4>{t('cacheTitle')}</h4>
            <button className="btn btn-default">{t('clean')}</button>
        </div>);
    }
}

export default translate('common', { wait: process && !process.release })(Cache);