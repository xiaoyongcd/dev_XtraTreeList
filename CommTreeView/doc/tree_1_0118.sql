/*
Navicat MySQL Data Transfer

Source Server         : 127_msqls
Source Server Version : 50627
Source Host           : localhost:3306
Source Database       : test

Target Server Type    : MYSQL
Target Server Version : 50627
File Encoding         : 65001

Date: 2019-01-18 11:04:10
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for tree_1
-- ----------------------------
DROP TABLE IF EXISTS `tree_1`;
CREATE TABLE `tree_1` (
  `_id` int(11) NOT NULL AUTO_INCREMENT,
  `n_id` int(11) DEFAULT NULL,
  `n_pid` int(11) DEFAULT NULL,
  `n_name` varchar(30) DEFAULT NULL,
  `n_data` text COMMENT 'json格式的业务数据',
  `n_desc` varchar(100) DEFAULT NULL,
  `n_order` int(11) DEFAULT NULL,
  `n_status` char(1) DEFAULT NULL COMMENT '1可用0禁用',
  `n_disable` char(1) DEFAULT NULL COMMENT '禁用(1能显示但变灰0正常)',
  `n_owner` varchar(4) DEFAULT NULL COMMENT '拥有者',
  `n_tester` varchar(4) DEFAULT NULL,
  `create_time` datetime DEFAULT NULL,
  PRIMARY KEY (`_id`),
  UNIQUE KEY `uq_id` (`n_id`)
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of tree_1
-- ----------------------------
INSERT INTO `tree_1` VALUES ('1', '1', null, '根', '{\"ename\":\"肖勇\"}', null, '1', '1', null, null, null, null);
INSERT INTO `tree_1` VALUES ('2', '2', '1', '子系统1', '{\"a\":\"b\"}', 'c', '0', '1', '0', '', '', '2019-01-18 10:58:32');
INSERT INTO `tree_1` VALUES ('3', '3', '1', '子系统2', '', '', '3', '1', '0', '', '', '2019-01-18 10:58:33');
INSERT INTO `tree_1` VALUES ('4', '4', '1', '模块11', '', '', '8', '1', '0', '11', '111', '2019-01-18 10:58:33');
INSERT INTO `tree_1` VALUES ('5', '5', '1', '模块12', '', '', '7', '1', '0', '', '', '2019-01-18 10:58:33');
INSERT INTO `tree_1` VALUES ('6', '6', '2', '模块21', '', '', '1', '1', '0', '', '', '2019-01-18 10:58:33');
INSERT INTO `tree_1` VALUES ('7', '7', '2', '模块22', '{\"xx\":\"aa\"}', 'c', '2', '1', '0', '', '', '2019-01-18 10:58:33');
INSERT INTO `tree_1` VALUES ('8', '8', '3', '模块133', '', '', '4', '1', '0', '', '', '2019-01-18 10:58:33');
INSERT INTO `tree_1` VALUES ('9', '9', '3', 'xxx', '{\"version\":\"1\",\"ename\":\"肖勇8\"}', '描述8xx', '5', '1', '1', '张三要要', '李要有', '2019-01-18 10:58:33');
INSERT INTO `tree_1` VALUES ('10', '10', '3', '模块33', '{\"version\":\"2\",\"ename\":\"a8\"}', '33加44', '6', '1', '0', '张三要', '三要', '2019-01-18 10:58:33');
INSERT INTO `tree_1` VALUES ('11', '11', '1', '模31块13', '{\"34\":\"35\"}', '33', '10', '1', '0', '31', '32', '2019-01-18 10:58:33');
INSERT INTO `tree_1` VALUES ('12', '12', '1', '模块14', '', '', '9', '1', '0', '', '', '2019-01-18 10:58:33');
