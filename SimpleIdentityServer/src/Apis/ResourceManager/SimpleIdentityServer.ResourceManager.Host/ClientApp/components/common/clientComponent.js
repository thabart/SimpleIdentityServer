import React, { Component } from "react";
import { translate } from 'react-i18next';
import { ClientService } from '../../services';

import Table, { TableBody, TableCell, TableHead, TableRow, TableFooter, TablePagination } from 'material-ui/Table';
import { Popover, IconButton, Menu, MenuItem, Checkbox, TextField, Select, Avatar, CircularProgress } from 'material-ui';
import MoreVert from '@material-ui/icons/MoreVert';
import Delete from '@material-ui/icons/Delete';

class ClientComponent extends Component {
    constructor(props) {
        super(props);
        this.handleClick = this.handleClick.bind(this);
        this.handleClose = this.handleClose.bind(this);
        this.handleChangeFilter = this.handleChangeFilter.bind(this);
        this.refreshData = this.refreshData.bind(this);
        this.getFilter = this.getFilter.bind(this);
        this.handleChangePage = this.handleChangePage.bind(this);
        this.handleChangeRowsPage = this.handleChangeRowsPage.bind(this);
        this.handleRowClick = this.handleRowClick.bind(this);
        this.handleAllSelections = this.handleAllSelections.bind(this);
        this.handleRemoveClients = this.handleRemoveClients.bind(this);

        this.state = {
            data: [],
            isLoading: false,
            page: 0,
            pageSize: 5,
            count: 0,
            anchorEl: null,
            isRemoveDisplayed: false,
            selectedId: null
        };
    }

    /**
    * Open the menu.
    */
    handleClick(e) {
        this.setState({
            anchorEl: e.currentTarget
        });
    }

    /**
    * Close the menu.
    */
    handleClose() {
        this.setState({
            anchorEl: null
        });
    }

    /**
    * Handle the change
    */
    handleChangeFilter(e) {
        var self = this;
        self.setState({
            [e.target.name]: e.target.value
        }, () => {
            var filter = self.getFilter();
            self.refreshData(self.state.page, self.state.pageSize, filter);
        });
    }

    /**
    * Refresh the clients.
    */
    refreshData(page, pageSize, filter) { 
        var startIndex = page * pageSize;
        var self = this;
        self.setState({
            isLoading: true
        });

        var request = { start_index: startIndex, count: pageSize };
        request['client_ids'] = filter.client_ids;
        ClientService.search(request, self.props.type).then(function (result) {
            var data = [];
            if (result.content) {
                result.content.forEach(function (client) {
                    data.push({
                        logo_uri: client['logo_uri'],
                        client_name: client['client_name'],
                        client_id: client['client_id'],
                        isSelected: false
                    });
                });
            }
            
            self.setState({
                isLoading: false,
                data: data,
                pageSize: pageSize,
                count: result.count
            });
        }).catch(function (e) {
            self.setState({
                isLoading: false,
                data: [],
                count: 0
            });
        });
    }

    /**
    * Get the filter.
    */
    getFilter() {  
        var filter = {
            client_ids: []
        };
        if (this.state.selectedId && this.state.selectedId !== '') {
            filter['client_ids'] = [ this.state.selectedId ];
        }

        return filter;
    }

    /**
    * Execute when the page has changed.
    */
    handleChangePage(evt, page) {
        this.setState({
            page: page
        });
        this.refreshData(page, this.state.pageSize, this.getFilter());
    }

    /**
    * Execute when the number of records has changed.
    */
    handleChangeRowsPage(evt) {
        this.setState({
            pageSize: evt.target.value
        });
        this.refreshData(this.state.page, evt.target.value, this.getFilter());
    }

    /**
    * Handle click on the row.
    */
    handleRowClick(e, record) {
        record.isSelected = e.target.checked;
        var data = this.state.data;
        var nbSelectedRecords = data.filter(function(r) { return r.isSelected; }).length;
        this.setState({
            data: data,
            isRemoveDisplayed: nbSelectedRecords > 0
        });
    }

    /**
    * Select / Unselect all scopes.
    */
    handleAllSelections(e) {
        var checked = e.target.checked;
        var data = this.state.data;
        data.forEach(function(r) { r.isSelected = checked ;});
        this.setState({
            data: data,
            isRemoveDisplayed: checked
        });
    }

    /**
    * Remove the selected clients.
    */
    handleRemoveClients() {      
    }


    render() {
        var self = this;
        const { t } = this.props;
        var rows = [];
        if (self.state.data) {
            self.state.data.forEach(function(record) {
                rows.push((
                    <TableRow hover role="checkbox" key={record.client_id}>
                        <TableCell><Checkbox checked={record.isSelected} onChange={(e) => self.handleRowClick(e, record)} /></TableCell>
                        <TableCell><Avatar src={record.logo_uri}/></TableCell>
                        <TableCell>{record.client_id}</TableCell>
                        <TableCell>{record.client_name}</TableCell>
                    </TableRow>
                ));
            });
        }

        return (<div className="block">
            <div className="block-header">
                <h4>{t('oauthClients')}</h4>
                <i>{t('oauthClientsShortDescription')}</i>
            </div>
            <div className="container-fluid">
                <div className="row">
                    <div className="col-md-12">
                        <div className="card">
                            <div className="header">
                                <h4 style={{display: "inline-block"}}>{t('listOfClients')}</h4>
                                <div style={{float: "right"}}>
                                    {self.state.isRemoveDisplayed && (
                                        <IconButton onClick={self.handleRemoveClients}>
                                            <Delete />
                                        </IconButton>
                                    )}
                                    <IconButton onClick={this.handleClick}>
                                        <MoreVert />
                                    </IconButton>
                                    <Menu anchorEl={self.state.anchorEl} open={Boolean(self.state.anchorEl)} onClose={self.handleClose}>
                                        <MenuItem>{t('addClient')}</MenuItem>
                                    </Menu>
                                </div>
                            </div>
                            <div className="body">
                                { this.state.isLoading ? (<CircularProgress />) : (
                                    <div>
                                        <Table>
                                            <TableHead>
                                                <TableRow>
                                                    <TableCell></TableCell>
                                                    <TableCell></TableCell>
                                                    <TableCell>{t('clientName')}</TableCell>
                                                    <TableCell>{t('clientId')}</TableCell>
                                                </TableRow>
                                            </TableHead>
                                            <TableBody>
                                                <TableRow>
                                                    <TableCell><Checkbox onChange={self.handleAllSelections} /></TableCell>
                                                    <TableCell></TableCell>
                                                    <TableCell></TableCell>
                                                    <TableCell><TextField value={this.state.selectedId} name='selectedId' onChange={this.handleChangeFilter} fullWidth={true} placeholder={t('Filter...')}/></TableCell>
                                                </TableRow>
                                                {rows}
                                            </TableBody>
                                        </Table>
                                        <TablePagination component="div" count={self.state.count} rowsPerPage={self.state.pageSize} page={this.state.page} onChangePage={self.handleChangePage} onChangeRowsPerPage={self.handleChangeRowsPage} />
                                    </div>
                                )}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>);
    }

    componentDidMount() {        
        this.refreshData(0, this.state.pageSize, this.getFilter());
    }
}

export default translate('common', { wait: process && !process.release })(ClientComponent);