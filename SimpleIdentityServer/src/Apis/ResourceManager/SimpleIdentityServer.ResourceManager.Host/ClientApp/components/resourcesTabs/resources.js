import React, { Component } from "react";
import { translate } from 'react-i18next';
import { ResourceService } from '../../services';

import Table, { TableBody, TableCell, TableHead, TableRow, TableFooter, TablePagination } from 'material-ui/Table';
import { Popover, IconButton, Menu, MenuItem, Checkbox, TextField, Select, Avatar, CircularProgress } from 'material-ui';
import Delete from '@material-ui/icons/Delete';
import MoreVert from '@material-ui/icons/MoreVert';

class Resources extends Component {
    constructor(props) {
        super(props);
        this.handleClick = this.handleClick.bind(this);
        this.handleClose = this.handleClose.bind(this);
        this.handleAllSelections = this.handleAllSelections.bind(this);
        this.handleRowClick = this.handleRowClick.bind(this);
        this.refreshData = this.refreshData.bind(this);
        this.handleRemoveResources = this.handleRemoveResources.bind(this);
        this.state = {
            isLoading: false,
            page: 0,
            pageSize: 5,
            isRemoveDisplayed: false,
            anchorEl: null,
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
    * Select all the data.
    */
    handleAllSelections(e) {
        var self = this;
        var checked = e.target.checked;
        var data = self.state.data;
        data.forEach(function(r) { r.isSelected = checked ;});
        self.setState({
            data: data,
            isRemoveDisplayed: checked
        });
    }

    /**
    * Handle click on the row.
    */
    handleRowClick(e, record) {
        var self  = this;
        record.isSelected = e.target.checked;
        var data = self.state.data;
        var nbSelectedRecords = data.filter(function(r) { return r.isSelected; }).length;
        self.setState({
            data: data,
            isRemoveDisplayed: nbSelectedRecords > 0
        });
    }

    /**
    * Refresh the resources.
    */
    refreshData() {
        var self = this;
        var startIndex = self.state.page * self.state.pageSize;
        self.setState({
            isLoading: true
        });

        var request = { start_index: startIndex, count: self.state.pageSize };
        ResourceService.search(request, self.props.type).then(function (result) {
            var data = [];
            console.log(result);
            if (result.content) {
                result.content.forEach(function (r) {
                    var scopes = r['scopes'] ? r['scopes'].join(',') : '-';
                    data.push({
                        id: r['_id'],
                        name: r['name'],
                        type: r['type'],
                        scopes: scopes,
                        isSelected: false
                    });
                });
            }
            
            console.log(data);
            self.setState({
                isLoading: false,
                data: data,
                count: result.count
            });
        }).catch(function (e) {
            console.log(e);
            self.setState({
                isLoading: false,
                data: []
            });
        });
    }

    /**
    * Remove the selected resources.
    */
    handleRemoveResources() {

    }

    render() {
        var self = this;
        const { t } = self.props;
        var rows = [];
        if (self.state.data) {
            self.state.data.forEach(function(record) {
                rows.push(
                    <TableRow>
                        <TableCell><Checkbox checked={record.isSelected} onChange={(e) => self.handleRowClick(e, record)} /></TableCell>
                        <TableCell>{record.id}</TableCell>
                        <TableCell>{record.name}</TableCell>
                        <TableCell>{record.type}</TableCell>
                        <TableCell>{record.scopes}</TableCell>
                    </TableRow>
                );
            });
        }

        return (<div className="card">
                <div className="header">
                    <h4 style={{display: "inline-block"}}>{t('resources')}</h4>
                    <div style={{float: "right"}}>
                        {self.state.isRemoveDisplayed && (
                            <IconButton onClick={self.handleRemoveResources}>
                                <Delete />
                            </IconButton>
                        )}
                        <IconButton onClick={this.handleClick}>
                            <MoreVert />
                        </IconButton>
                        <Menu anchorEl={self.state.anchorEl} open={Boolean(self.state.anchorEl)} onClose={self.handleClose}>
                            <MenuItem>{t('addResource')}</MenuItem>
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
                                        <TableCell>{t('resourceId')}</TableCell>
                                        <TableCell>{t('resourceName')}</TableCell>
                                        <TableCell>{t('resourceType')}</TableCell>
                                        <TableCell>{t('resourceScopes')}</TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    <TableRow>
                                        <TableCell><Checkbox onChange={self.handleAllSelections} /></TableCell>
                                        <TableCell></TableCell>
                                        <TableCell></TableCell>
                                        <TableCell></TableCell>
                                        <TableCell></TableCell>
                                    </TableRow>
                                    {rows}
                                </TableBody>
                            </Table>
                            <TablePagination component="div" count={self.state.count} rowsPerPage={self.state.pageSize} page={this.state.page} onChangePage={self.handleChangePage} onChangeRowsPerPage={self.handleChangeRowsPage} />
                        </div>
                    )}
                </div>
        </div>);
    }

    componentDidMount() {
        this.refreshData();
    }
}

export default translate('common', { wait: process && !process.release })(Resources);